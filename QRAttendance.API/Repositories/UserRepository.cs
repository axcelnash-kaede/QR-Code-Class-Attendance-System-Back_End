using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using QRAttendance.API.Models;

public class UserRepository
{
    private readonly IConfiguration _config;

    public UserRepository(IConfiguration config)
    {
        _config = config;
    }

    private IDbConnection CreateConnection()
        => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

    // GET USER BY EMAIL
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = CreateConnection();

        var sql = @"SELECT Id, StudentId, FullName, Email, Password, Role
                    FROM Users
                    WHERE Email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(
            sql,
            new { Email = email }
        );
    }

    // CHECK EMAIL EXISTS
    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = CreateConnection();

        var sql = @"SELECT COUNT(1)
                    FROM Users
                    WHERE Email = @Email";

        var count = await connection.ExecuteScalarAsync<int>(
            sql,
            new { Email = email }
        );

        return count > 0;
    }

    // CHECK STUDENT ID EXISTS
    public async Task<bool> StudentIdExistsAsync(string studentId)
    {
        using var connection = CreateConnection();

        var sql = @"SELECT COUNT(1)
                    FROM Users
                    WHERE StudentId = @StudentId";

        var count = await connection.ExecuteScalarAsync<int>(
            sql,
            new { StudentId = studentId }
        );

        return count > 0;
    }

    public async Task RegisterAsync(User user)
    {
        var sql = @"EXEC SP_QRAttendanceDB_RegisterUser
                @StudentId,
                @FullName,
                @Email,
                @Password,
                @Role";

        using var connection = CreateConnection();

        await connection.ExecuteAsync(sql, new
        {
            user.StudentId,
            user.FullName,
            user.Email,
            user.Password,
            user.Role
        });
    }
}