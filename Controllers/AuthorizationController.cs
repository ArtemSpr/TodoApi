using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace UserAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, string> Users = new();

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto user)
        {
            var hashedPassword = HashPassword(user.Password);
            if (Users.TryAdd(user.Username, hashedPassword))
            {
                return Ok("User registered successfully.");
            }
            return Conflict("Username already exists.");
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] UserDto user)
        {
            if (Users.TryGetValue(user.Username, out var storedHashedPassword))
            {
                return storedHashedPassword == HashPassword(user.Password)
                    ? Ok("Valid credentials.")
                    : Unauthorized();
            }
            return NotFound("User not found.");
        }
    }

    public record UserDto(string Username, string Password);
}
