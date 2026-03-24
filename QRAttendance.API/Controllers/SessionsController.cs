using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRAttendance.API.Data;
using QRAttendance.API.Models;

namespace QRAttendance.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class SessionsController : ControllerBase
{
    private readonly DapperContext _context;

    public SessionsController(DapperContext context)
    {
        _context = context;
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        using var connection = _context.CreateConnection();

        var sessions = await connection.QueryAsync<AttendanceSession>(
            "SELECT * FROM AttendanceSessions");

        return Ok(sessions);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(int id)
    {
        using var connection = _context.CreateConnection();

        var session = await connection.QueryFirstOrDefaultAsync<AttendanceSession>(
            "SELECT * FROM AttendanceSessions WHERE Id = @Id",
            new { Id = id });

        if (session == null) return NotFound();

        return Ok(session);
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> CreateSession()
    {
        using var connection = _context.CreateConnection();

        var sql = @"
                INSERT INTO AttendanceSessions(IsActive, CreatedAt)
                VALUES(1, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() as int);
            ";

        var id = await connection.ExecuteScalarAsync<int>(sql);

        return Ok(new { Id = id, Message = "Session created" });
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSession(int id, AttendanceSession model)
    {
        using var connection = _context.CreateConnection();

        var rows = await connection.ExecuteAsync(
            @"UPDATE AttendanceSessions
                  SET IsClosed = @IsClosed
                  WHERE Id = @Id",
            new { model.IsClosed, Id = id });

        if (rows == 0) return NotFound();

        return Ok("Session updated successfully");
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        using var connection = _context.CreateConnection();

        var rows = await connection.ExecuteAsync(
            "DELETE FROM AttendanceSessions WHERE Id = @Id",
            new { Id = id });

        if (rows == 0) return NotFound();

        return Ok("Session deleted successfully");
    }
}