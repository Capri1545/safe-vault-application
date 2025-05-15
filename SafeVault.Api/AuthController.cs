using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace SafeVault.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // This is a placeholder. In production, use a secure user store and hashed passwords.
        private static readonly List<UserRecord> Users = new()
        {
            new UserRecord
            {
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "Admin",
            },
            new UserRecord
            {
                Username = "user1",
                Password = BCrypt.Net.BCrypt.HashPassword("userpass"),
                Role = "User",
            },
        };

        public class UserRecord
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "User";
        }

        public record LoginRequest([Required] string Username, [Required] string Password);

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Basic input validation
            if (
                string.IsNullOrWhiteSpace(request.Username)
                || string.IsNullOrWhiteSpace(request.Password)
            )
            {
                return BadRequest(new { Message = "Username and password are required." });
            }

            // Simulate user lookup and password verification
            var user = Users.FirstOrDefault(u => u.Username == request.Username);
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
            // Return a list of users (excluding passwords for security)
            var userList = Users.Select(u => new { u.Username, u.Role }).ToList();
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
            {
                return BadRequest(new { Message = "Username and password are required." });
            }
            if (Users.Any(u => u.Username == newUser.Username))
            {
                return Conflict(new { Message = "User already exists." });
            }
            Users.Add(
                new UserRecord
                {
                    Username = newUser.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                    Role = newUser.Role ?? "User",
                }
            );
            return Ok(new { Message = "User added successfully." });
        }

        [HttpDelete("delete-user/{username}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(string username)
        {
            var user = Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }
            if (user.Role == "Admin")
            {
                return BadRequest(new { Message = "Cannot delete admin user." });
            }
            Users.Remove(user);
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
