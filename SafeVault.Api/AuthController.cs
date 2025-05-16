using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Api.Data;

namespace SafeVault.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseInitializer _db;
        private readonly IConfiguration _config;

        public AuthController(DatabaseInitializer db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public class UserRecord
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "User";
            public string Email { get; set; } = string.Empty;
        }

        public record LoginRequest([Required] string Username, [Required] string Password);

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (
                string.IsNullOrWhiteSpace(request.Username)
                || string.IsNullOrWhiteSpace(request.Password)
            )
                return BadRequest(new { Message = "Username and password are required." });

            // Look up user from DB
            var user = _db.GetUserByUsername(request.Username);
            if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                // Generate JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("SuperSecretKey12345SuperSecretKey12345"); // 32 chars = 256 bits
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                };
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    ),
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                return Ok(
                    new
                    {
                        Token = jwtToken,
                        Username = user.Username,
                        Role = user.Role,
                    }
                );
            }
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            var users = _db.GetAllUsers();
            var userList = users
                .Select(u => new
                {
                    username = u.Username,
                    email = u.Email,
                    role = u.Role,
                })
                .ToList();
            return Ok(userList);
        }

        [HttpPost("add-user")]
        [Authorize(Roles = "Admin")]
        public IActionResult AddUser([FromBody] UserRecord newUser)
        {
            if (
                string.IsNullOrWhiteSpace(newUser.Username)
                || string.IsNullOrWhiteSpace(newUser.Password)
            )
                return BadRequest(new { Message = "Username and password are required." });
            if (_db.GetUserByUsername(newUser.Username) != null)
                return Conflict(new { Message = "User already exists." });
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
            _db.AddUser(newUser.Username, hashedPassword, newUser.Role ?? "User", newUser.Email);
            return Ok(new { Message = "User added successfully." });
        }

        [HttpDelete("delete-user/{username}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(string username)
        {
            var user = _db.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }
            if (user.Role == "Admin")
            {
                return BadRequest(new { Message = "Cannot delete admin user." });
            }
            _db.DeleteUser(username);
            return Ok(new { Message = "User deleted successfully." });
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            var username = User.Identity?.Name;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return Ok(new { Message = $"Hello, {username}. Your role is {role}." });
        }
    }
}
