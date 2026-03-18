using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using QRAttendance.API.DTOs;

public class DashboardRepository
{
    private readonly IConfiguration _config;

    public DashboardRepository(IConfiguration config)
    {
        _config = config;
    }

    private IDbConnection CreateConnection()
        => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

    // ================= TEACHER =================
    public async Task<int> GetTotalSessions()
    {
        var sql = "SELECT COUNT(*) FROM AttendanceSessions";

        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetActiveSessions()
    {
        var sql = "SELECT COUNT(*) FROM AttendanceSessions WHERE IsActive = 1";

        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetTotalAttendanceRecords()
    {
        var sql = "SELECT COUNT(*) FROM AttendanceRecords";

        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    // ================= STUDENT =================
    public async Task<StudentDashboardDto> GetStudentDashboard(int studentId)
    {
        using var conn = CreateConnection();

        // total sessions
        var totalSessions = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM AttendanceSessions"
        );

        // sessions attended
        var attended = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(DISTINCT SessionId)
          FROM AttendanceRecords
          WHERE StudentId = @StudentId",
            new { StudentId = studentId }
        );

        var missed = totalSessions - attended;

        double percentage = 0;

        if (totalSessions > 0)
            percentage = (double)attended / totalSessions * 100;

        return new StudentDashboardDto
        {
            TotalSessions = totalSessions,
            SessionsAttended = attended,
            SessionsMissed = missed,
            AttendancePercentage = Math.Round(percentage, 2)
        };
    }

    // ================= TEACHER =================
    public async Task<TeacherDashboardDto> GetTeacherDashboard()
    {
        using var conn = CreateConnection();

        var totalSessions = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM AttendanceSessions"
        );

        var activeSessions = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM AttendanceSessions WHERE IsActive = 1"
        );

        var closedSessions = totalSessions - activeSessions;

        var totalAttendance = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM AttendanceRecords"
        );

        var totalStudents = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Users WHERE Role = 'Student'"
        );

        return new TeacherDashboardDto
        {
            TotalSessions = totalSessions,
            ActiveSessions = activeSessions,
            ClosedSessions = closedSessions,
            TotalAttendanceRecords = totalAttendance,
            TotalStudents = totalStudents
        };
    }
}