using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PaymentAPI.Data;
using PaymentAPI.Models;
using BCrypt.Net;

namespace PaymentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { message = "Username is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Password is required" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters" });
            }

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Hash the password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = request.Role ?? "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", username = user.Username });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequest request)
        {
            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                username = user.Username,
                role = user.Role
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}