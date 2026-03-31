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
    public async Task<IActionResult> ScanQR(ScanQrDto dto)
    {
        int studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _service.ScanQR(
            dto.QrContent,
            studentId,
            dto.DeviceId
        );

        return Ok(result);
    }

    // TEACHER CREATE SESSION

    [HttpPost("create-session")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateSession(CreateAttendanceSessionDto dto)
    {
        var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _service.CreateSession(dto, teacherId);

        var response = new
        {
            SessionId = result.SessionId,
            QrCode = result.QrCode,
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

    [HttpGet("qr/{sessionId}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetQrCode(int sessionId)
    {
        var qr = await _service.GetQRCode(sessionId);

        if (qr == null)
            return NotFound("Session not found");

        return Ok(qr);
      }
}