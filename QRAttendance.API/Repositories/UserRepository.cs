using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using QRAttendance.API.Models;

namespace QRAttendance.API.Repositories
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<User>(
                "dbo.SP_QRAttendanceDB_LoginUser",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();

            var sql = @"SELECT Id, StudentId, FullName, Email, PasswordHash, Role, DeviceId, CreatedAt
                        FROM dbo.Users
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = CreateConnection();

            var sql = @"SELECT COUNT(1)
                        FROM dbo.Users
                        WHERE Email = @Email";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> StudentIdExistsAsync(string studentId)
        {
            using var connection = CreateConnection();

            var sql = @"SELECT COUNT(1)
                        FROM dbo.Users
                        WHERE StudentId = @StudentId";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { StudentId = studentId });
            return count > 0;
        }

        public async Task RegisterAsync(User user)
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync(
                "dbo.SP_QRAttendanceDB_RegisterUser",
                new
                {
                    user.StudentId,
                    user.FullName,
                    user.Email,
                    user.PasswordHash,
                    user.Role
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdateStudentDeviceAsync(int userId, string deviceId)
        {
            using var connection = CreateConnection();

            var sql = @"UPDATE dbo.Users
                        SET DeviceId = @DeviceId
                        WHERE Id = @UserId";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                DeviceId = deviceId
            });
        }
    }
}