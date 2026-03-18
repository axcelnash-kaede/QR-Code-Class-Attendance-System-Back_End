using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRAttendance.API.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // ======================================================
    // 👨‍🏫 TEACHER - CREATE SESSION
    // ======================================================
    [Authorize(Roles = "Teacher")]
    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateAttendanceSessionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required");

        var sessionId = await _attendanceService.CreateSessionAsync(dto.Title);

        return Ok(new { SessionId = sessionId });
    }

    // ======================================================
    // 👨‍🏫 TEACHER - GET ALL SESSIONS
    // ======================================================
    [Authorize(Roles = "Teacher")]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetAllSessions()
    {
        var sessions = await _attendanceService.GetAllSessionsAsync();
        return Ok(sessions);
    }

    // ======================================================
    // 👨‍🎓 STUDENT - MARK ATTENDANCE
    // ======================================================
    [Authorize(Roles = "Student")]
    [HttpPost("mark-attendance/{sessionId}")]
    public async Task<IActionResult> MarkAttendance(int sessionId)
    {
        try
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var studentName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            await _attendanceService.MarkAttendanceAsync(
                sessionId,
                studentId,
                studentName
            );

            return Ok("Attendance recorded successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // ======================================================
    // 👨‍🏫 TEACHER - VIEW ATTENDANCE
    // ======================================================
    [Authorize(Roles = "Teacher")]
    [HttpGet("view-attendance/{sessionId}")]
    public async Task<IActionResult> ViewAttendance(int sessionId)
    {
        var records = await _attendanceService.GetAttendanceBySessionAsync(sessionId);
        return Ok(records);
    }

    // ======================================================
    // 👨‍🎓 STUDENT - VIEW MY ATTENDANCE
    // ======================================================
    [Authorize(Roles = "Student")]
    [HttpGet("my-attendance")]
    public async Task<IActionResult> GetMyAttendance()
    {
        var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var records = await _attendanceService.GetStudentAttendanceAsync(studentId);

        return Ok(records);
    }

    // ======================================================
    // 👨‍🏫 TEACHER - DELETE ATTENDANCE
    // ======================================================
    [Authorize(Roles = "Teacher")]
    [HttpDelete("attendance/{id}")]
    public async Task<IActionResult> DeleteAttendance(int id)
    {
        await _attendanceService.DeleteAttendanceAsync(id);
        return Ok("Attendance deleted successfully");
    }

    // ======================================================
    // 👨‍🏫 TEACHER - CLOSE SESSION
    // ======================================================
    [Authorize(Roles = "Teacher")]
    [HttpPost("close-session/{sessionId}")]
    public async Task<IActionResult> CloseSession(int sessionId)
    {
        await _attendanceService.CloseSessionAsync(sessionId);
        return Ok("Session closed successfully");
    }
}