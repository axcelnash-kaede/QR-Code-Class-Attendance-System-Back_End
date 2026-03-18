using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRAttendance.API.Data;
using QRAttendance.API.Models;

namespace QRAttendanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class UsersController : ControllerBase
    {
        private readonly DapperContext _context;

        public UsersController(DapperContext context)
        {
            _context = context;
        }

        // GET ALL USERS
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            using var connection = _context.CreateConnection();

            var users = await connection.QueryAsync(
                @"SELECT Id, FullName, Email, Role
                  FROM Users");

            return Ok(users);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            using var connection = _context.CreateConnection();

            var user = await connection.QueryFirstOrDefaultAsync(
                @"SELECT Id, FullName, Email, Role
                  FROM Users
                  WHERE Id = @Id",
                new { Id = id });

            if (user == null) return NotFound();

            return Ok(user);
        }

        // CREATE USER
        [HttpPost]
        public async Task<IActionResult> CreateUser(User model)
        {
            using var connection = _context.CreateConnection();

            var hashed = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await connection.ExecuteAsync(
                @"INSERT INTO Users
                  (StudentId, FullName, Email, Password, Role)
                  VALUES (@StudentId, @FullName, @Email, @Password, @Role)",
                new
                {
                    model.StudentId,
                    model.FullName,
                    model.Email,
                    Password = hashed,
                    model.Role
                });

            return Ok("User created successfully");
        }

        // UPDATE USER
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User model)
        {
            using var connection = _context.CreateConnection();

            string sql = @"
                UPDATE Users
                SET FullName=@FullName,
                    Email=@Email,
                    Role=@Role";

            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                sql += ", Password=@Password";
            }

            sql += " WHERE Id=@Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                model.FullName,
                model.Email,
                model.Role,
                model.Password
            });

            return Ok("User updated successfully");
        }

        // DELETE USER
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(
                "DELETE FROM Users WHERE Id=@Id",
                new { Id = id });

            return Ok("User deleted successfully");
        }
    }
}