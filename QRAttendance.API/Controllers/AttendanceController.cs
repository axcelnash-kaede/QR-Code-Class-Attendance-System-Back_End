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
        public async Task<IActionResult> ScanQR([FromBody] ScanQrDto dto)
        {
            int studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string? userAgent = Request.Headers.UserAgent.ToString();

            var result = await _service.ScanQR(
                dto.QrContent,
                studentId,
                dto.DeviceId,
                userAgent
            );

            if (result.Success)
                return Ok(result);

            return result.Code switch
            {
                "INVALID_QR" => BadRequest(result),
                "INVALID_FORMAT" => BadRequest(result),
                "INVALID_SESSION_ID" => BadRequest(result),
                "INVALID_TOKEN" => BadRequest(result),
                "SESSION_NOT_FOUND" => NotFound(result),
                "SESSION_CLOSED" => BadRequest(result),
                "SESSION_INACTIVE" => BadRequest(result),
                "SESSION_EXPIRED" => BadRequest(result),
                "SUBJECT_MISSING" => BadRequest(result),
                "NOT_ENROLLED" => Forbid(),
                "STUDENT_NOT_FOUND" => NotFound(result),
                "SECTION_MISSING" => BadRequest(result),
                "WRONG_SECTION" => BadRequest(result),
                "SUSPICIOUS_DEVICE" => StatusCode(StatusCodes.Status403Forbidden, result),
                "DUPLICATE" => Conflict(result),
                _ => BadRequest(result)
            };
        }

        // TEACHER CREATE SESSION
        [HttpPost("create-session")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateSession([FromBody] CreateAttendanceSessionDto dto)
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.CreateSession(dto, teacherId);

            return Ok(new
            {
                sessionId = result.SessionId,
                qrCode = result.QrCode,
                message = "Session created successfully"
            });
        }

        // TEACHER CLOSE SESSION
        [HttpPost("close")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CloseSession([FromQuery] int sessionId)
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.CloseSession(sessionId, teacherId);

            if (!result.Success)
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });

            return Ok(new
            {
                success = true,
                message = result.Message
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

            return Ok(new
            {
                qrCode = qr
            });
        }

        // TEACHER GET SECTIONS WITH SUBJECTS
        [HttpGet("teacher-sections-subjects")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTeacherSectionSubjects()
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.GetTeacherSectionSubjectsAsync(teacherId);

            return Ok(result);
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