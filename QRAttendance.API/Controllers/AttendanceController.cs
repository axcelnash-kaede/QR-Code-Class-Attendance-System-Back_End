using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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

    // TEACHER CLOSE SESSION
    [HttpPost("close")]
    public async Task<IActionResult> CloseSession(int sessionId)
    {
        var result = await _service.CloseSession(sessionId);

        return Ok(result);
    }
}