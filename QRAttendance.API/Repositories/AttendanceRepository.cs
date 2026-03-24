using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using QRAttendance.API.Data;
using QRAttendance.API.Models;


namespace QRAttendanceAPI.Repositories;

public class AttendanceRepository
{
    private readonly DapperContext _context;

    public AttendanceRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<AttendanceSession?> GetSession(int sessionId)
    {
        var query = @"
SELECT *
FROM AttendanceSession
WHERE Id = @Id
";

        using var connection = _context.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AttendanceSession>(
            query,
            new { Id = sessionId }
        );
    }

    public async Task<AttendanceRecord?> GetAttendance(int sessionId, int studentId)
    {
        var query = @"
SELECT *
FROM AttendanceRecords
WHERE SessionId = @SessionId
AND StudentId = @StudentId
";

        using var connection = _context.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AttendanceRecord>(
            query,
            new
            {
                SessionId = sessionId,
                StudentId = studentId
            }
        );
    }

    public async Task InsertAttendance(
        int sessionId,
        int studentId,
        string status,
        string deviceId
    )
    {
        var query = @"
INSERT INTO AttendanceRecords
(
    SessionId,
    StudentId,
    Status,
    DeviceId
)
VALUES
(
    @SessionId,
    @StudentId,
    @Status,
    @DeviceId
)
";

        using var connection = _context.CreateConnection();

        await connection.ExecuteAsync(
            query,
            new
            {
                SessionId = sessionId,
                StudentId = studentId,
                Status = status,
                DeviceId = deviceId
            }
        );
    }
    public async Task UpdateDevice(int studentId, string deviceId)
    {
        var sql = "UPDATE users SET device_id = @deviceId WHERE user_id = @studentId";
        await GetDb().ExecuteAsync(sql, new { studentId, deviceId });
    }

    private static object GetDb()
    {
        return _db;
    }

    public async Task MarkAttendance(int studentId, int sessionId, DateTime time, string status)
    {
        var sql = @"INSERT INTO attendance_records (student_id, session_id, time_recorded, status)
                VALUES (@studentId, @sessionId, @time, @status)";

        await _db.ExecuteAsync(sql, new { studentId, sessionId, time, status });
    }
    public async Task<User> GetStudent(int studentId)
    {
        var sql = "SELECT * FROM users WHERE user_id = @studentId";
        return await _db.QueryFirstOrDefaultAsync<User>(sql, new { studentId });
    }

    public async Task<bool> HasStudentScanned(int studentId, int sessionId)
    {
        var sql = "SELECT COUNT(*) FROM attendance_records WHERE student_id = @studentId AND session_id = @sessionId";
        var result = await _db.ExecuteScalarAsync<int>(sql, new { studentId, sessionId });
        return result > 0;
    }
    public async Task CloseSession(int sessionId)
    {
        var query = @"
UPDATE AttendanceSession
SET IsClosed = 1
WHERE Id = @Id
";

        using var connection = _context.CreateConnection();

        await connection.ExecuteAsync(
            query,
            new { Id = sessionId }
        );
    }

    public async Task MarkAbsentStudents(int sessionId)
    {
        var query = @"
INSERT INTO AttendanceRecords
(
    SessionId,
    StudentId,
    Status
)
SELECT
    @SessionId,
    u.Id,
    'Absent'
FROM Users u
WHERE u.Role = 'Student'
AND NOT EXISTS
(
    SELECT 1
    FROM AttendanceRecords a
    WHERE a.SessionId = @SessionId
    AND a.StudentId = u.Id
)
";

        using var connection = _context.CreateConnection();

        await connection.ExecuteAsync(
            query,
            new { SessionId = sessionId }
        );
    }
}