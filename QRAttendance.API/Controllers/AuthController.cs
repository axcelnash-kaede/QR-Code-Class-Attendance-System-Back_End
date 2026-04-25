using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QRAttendance.API.DTOs;
using QRAttendance.API.Models;
using QRAttendance.API.Repositories;

namespace QRAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserRepository _repo;

        public AuthController(IConfiguration configuration, UserRepository repo)
        {
            _configuration = configuration;
            _repo = repo;
        }

        // REGISTER STUDENT
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _repo.EmailExistsAsync(dto.Email))
                return BadRequest("Email already exists");

            if (string.IsNullOrWhiteSpace(dto.StudentId) || dto.StudentId.Length != 4)
                return BadRequest("Student number must be 4 digits");

            var formattedStudentId = $"C24-01-{dto.StudentId}-MAN121";

            if (await _repo.StudentIdExistsAsync(formattedStudentId))
                return BadRequest("Student ID already exists");

            var user = new User
            {
                StudentId = formattedStudentId,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Student",
                SectionId = dto.SectionId
            };

            await _repo.RegisterAsync(user);

            return Ok(new
            {
                message = "Student registered successfully",
                studentId = formattedStudentId
            });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            var jwtSettings = _configuration.GetSection("Jwt");

            var jwtKey = jwtSettings["Key"]!;
            var jwtIssuer = jwtSettings["Issuer"]!;
            var jwtAudience = jwtSettings["Audience"]!;
            var duration = Convert.ToDouble(jwtSettings["DurationInMinutes"]);

            var key = Encoding.UTF8.GetBytes(jwtKey);
            var expiration = DateTime.UtcNow.AddMinutes(duration);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("StudentId", user.StudentId ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiration,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiration,
                role = user.Role,
                name = user.FullName,
                studentId = user.StudentId
            });
        }
    }
}