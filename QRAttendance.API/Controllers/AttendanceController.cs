using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRAttendance.API.DTOs;
using QRAttendance.API.Services;

namespace QRAttendance.API.Controllers
{
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
            string? userAgent = Request.Headers.UserAgent.ToString();

            var result = await _service.ScanQR(
                dto.QrContent,
                studentId,
                dto.DeviceId,
                userAgent
            );

            return Ok(new
            {
                message = result
            });
        }

        // TEACHER CREATE SESSION
        [HttpPost("create-session")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateSession(CreateAttendanceSessionDto dto)
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.CreateSession(dto, teacherId);

            return Ok(new
            {
                SessionId = result.SessionId,
                QrCode = result.QrCode,
                Message = "Session Created Successfully"
            });
        }

        // TEACHER CLOSE SESSION
        [HttpPost("close")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CloseSession([FromQuery] int sessionId)
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.CloseSession(sessionId, teacherId);

            return Ok(new
            {
                message = result

            });
        }

        // TEACHER GET QR CODE
        [HttpGet("qr/{sessionId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetQrCode(int sessionId)
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var qr = await _service.GetQRCode(sessionId, teacherId);

            if (qr == null)
                return NotFound("Session not found or you are not allowed to access it.");

            return Ok(qr);
        }

        // TEACHER VIEW ALL DEVICE LOGS
        [HttpGet("device-logs")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetDeviceLogs([FromQuery] string? status = null)
        {
            var logs = await _service.GetDeviceLogs(status);

            return Ok(new
            {
                count = logs.Count(),
                logs
            });
        }

        // TEACHER VIEW ONLY SUSPICIOUS LOGS
        [HttpGet("suspicious-logs")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetSuspiciousLogs()
        {
            var logs = await _service.GetSuspiciousLogs();

            return Ok(new
            {
                count = logs.Count(),
                logs
            });
        }
    }
}