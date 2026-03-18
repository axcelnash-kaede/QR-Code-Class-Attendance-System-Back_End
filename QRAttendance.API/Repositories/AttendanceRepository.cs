using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using QRAttendance.API.Models;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly IConfiguration _config;

    public AttendanceRepository(IConfiguration config)
    {
        _config = config;
    }

    // ======================================================
    // DATABASE CONNECTION
    // ======================================================
    private IDbConnection CreateConnection()
        => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

    // ======================================================
    // SESSIONS
    // ======================================================
    public async Task<int> CreateSessionAsync(string title)
    {
        using var connection = CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            "SP_QRAttendanceDB_CreateSession",
            new { Title = title },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<AttendanceSession>> GetAllSessionsAsync()
    {
        using var connection = CreateConnection();

        return await connection.QueryAsync<AttendanceSession>(
            "SP_QRAttendanceDB_GetAllSessions",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<AttendanceSession?> GetSessionByIdAsync(int sessionId)
    {
        using var connection = CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AttendanceSession>(
            "SP_QRAttendanceDB_GetSessionById",
            new { SessionId = sessionId },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task UpdateSessionAsync(int id, string title, bool isActive)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            "SP_QRAttendanceDB_UpdateSession",
            new
            {
                SessionId = id,
                Title = title,
                IsActive = isActive
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            "SP_QRAttendanceDB_DeleteSession",
            new { SessionId = sessionId },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task CloseSessionAsync(int sessionId)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            "SP_QRAttendanceDB_CloseSession",
            new { SessionId = sessionId },
            commandType: CommandType.StoredProcedure
        );
    }

    // ======================================================
    // ATTENDANCE
    // ======================================================
    public async Task<MarkAttendanceResult> MarkAttendanceAsync(
    int sessionId,
    int studentId,
    string studentName)
    {
        using var connection = CreateConnection();

        return await connection.QuerySingleAsync<MarkAttendanceResult>(
            "SP_QRAttendanceDB_MarkAttendance",
            new
            {
                SessionId = sessionId,
                StudentId = studentId,
                StudentName = studentName
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<AttendanceRecord>> GetAttendanceBySessionAsync(int sessionId)
    {
        using var connection = CreateConnection();

        return await connection.QueryAsync<AttendanceRecord>(
            "SP_QRAttendanceDB_GetAttendanceBySession",
            new { SessionId = sessionId },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<AttendanceRecord>> GetStudentAttendanceAsync(int studentId)
    {
        using var connection = CreateConnection();

        return await connection.QueryAsync<AttendanceRecord>(
            "SP_QRAttendanceDB_GetStudentAttendance",
            new { StudentId = studentId },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteAttendanceAsync(int attendanceId)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            "SP_QRAttendanceDB_DeleteAttendance",
            new { AttendanceId = attendanceId },
            commandType: CommandType.StoredProcedure
        );
    }
}