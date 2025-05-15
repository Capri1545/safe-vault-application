using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // This is a placeholder. In production, use a secure user store and hashed passwords.
    private static readonly Dictionary<string, string> Users = new()
    {
        { "admin", "password123" }, // Example only
    };

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
        if (
            Users.TryGetValue(request.Username, out var storedPassword)
            && storedPassword == request.Password
        )
        {
            // In production, return a JWT or secure token and set user roles/claims
            return Ok(
                new
                {
                    Message = "Login successful",
                    Username = request.Username,
                    Role = request.Username == "admin" ? "Admin" : "User",
                }
            );
        }
        return Unauthorized(new { Message = "Invalid username or password" });
    }
}
