using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRAttendance.API.DTOs;
using QRAttendance.API.Services;

namespace QRAttendance.API.Controllers;

[Route("api/attendance")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _service;

    public AttendanceController(AttendanceService service)
    {
        _service = service;
    }

    // STUDENT SCAN QR
    [HttpPost("scan")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> ScanQR(
        int sessionId,
        int studentId,
        string deviceId
    )
    {
        var result = await _service.ScanQR(
            sessionId,
            studentId,
            deviceId
        );

        return Ok(result);
    }

    // TEACHER CREATE SESSION

    [HttpPost("create-session")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateSession(CreateAttendanceSessionDto dto)
    {
        var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var newSessionId = await _service.CreateSession(dto, teacherId);

        var response = new
        {
            SessionId = newSessionId,
            Message = "Session Created Successfully"
    };

        return Ok(response);
    }

    // TEACHER CLOSE SESSION
    [HttpPost("close")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CloseSession(int sessionId)
    {
        var result = await _service.CloseSession(sessionId);

        return Ok(result);
    }
    
    // QR CODE

    [HttpGet("qrcode/{sessionId}")]
    public async Task<IActionResult> GetQrCode(int sessionId)
    {
        var imageBytes = await _service.GenerateQrCodeAsync(sessionId);

        if (imageBytes == null)
            return NotFound("Session not found");

        return File(imageBytes, "image/png");
    }
}