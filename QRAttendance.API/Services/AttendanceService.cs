using Microsoft.Data.SqlClient;
using QRAttendance.API.DTOs;
using QRAttendance.API.Helpers;
using QRAttendance.API.Repositories;

namespace QRAttendance.API.Services
{
    public class AttendanceService
    {
        private readonly AttendanceRepository _repo;
        private readonly UserRepository _userRepo;

        public AttendanceService(AttendanceRepository repo, UserRepository userRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
        }

        // STUDENT SCAN QR
        public async Task<ScanResultDto> ScanQR(string qrContent, int studentId, string? deviceId, string? userAgent)
        {
            deviceId = NormalizeDeviceId(deviceId);

            if (string.IsNullOrWhiteSpace(qrContent))
            {
                await _repo.LogDeviceScanAsync(studentId, null, deviceId, "Blocked", "QR content is required.");
                return ScanResultDto.Fail("QR content is required.", "INVALID_QR");
            }

            // BETTER DEVICE ID GENERATION
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = NormalizeDeviceId(DeviceIdHelper.GenerateStableDeviceId(studentId, userAgent));
            }

            var parts = qrContent.Split('|');

            if (parts.Length != 2)
            {
                await _repo.LogDeviceScanAsync(studentId, null, deviceId, "Blocked", "Invalid QR format.");
                return ScanResultDto.Fail("Invalid QR format.", "INVALID_FORMAT");
            }

            if (!int.TryParse(parts[0], out int sessionId))
            {
                await _repo.LogDeviceScanAsync(studentId, null, deviceId, "Blocked", "Invalid QR session ID.");
                return ScanResultDto.Fail("Invalid session ID.", "INVALID_SESSION_ID");
            }

            string qrToken = parts[1]?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(qrToken))
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Invalid QR token.");
                return ScanResultDto.Fail("Invalid QR token.", "INVALID_TOKEN");
            }

            var session = await _repo.GetSession(sessionId);

            if (session == null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session not found.");
                return ScanResultDto.Fail("Session not found.", "SESSION_NOT_FOUND");
            }

            if (session.IsClosed == true)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session is already closed.");
                return ScanResultDto.Fail("Session is already closed.", "SESSION_CLOSED");
            }

            if (!session.IsActive)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session is not active.");
                return ScanResultDto.Fail("Session is not active.", "SESSION_INACTIVE");
            }

            var now = DateTime.Now;

            if (now > session.ExpirationTime)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session has expired.");
                return ScanResultDto.Fail("QR code has expired.", "SESSION_EXPIRED");
            }

            if (!string.Equals(session.QrToken, qrToken, StringComparison.Ordinal))
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Invalid QR token.");
                return ScanResultDto.Fail("Invalid QR token.", "INVALID_TOKEN");
            }

            if (session.SubjectId == null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session subject is missing.");
                return ScanResultDto.Fail("Session subject missing.", "SUBJECT_MISSING");
            }

            var student = await _userRepo.GetByIdAsync(studentId);
            if (student == null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Student not found.");
                return ScanResultDto.Fail("Student not found.", "STUDENT_NOT_FOUND");
            }

            string? registeredDeviceId = NormalizeDeviceId(student.DeviceId);

            // SECTION VALIDATION
            if (student.SectionId == null || session.SectionId == null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Section data missing.");
                return ScanResultDto.Fail("Section data missing.", "SECTION_MISSING");
            }

            if (student.SectionId != session.SectionId)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Student belongs to a different section.");
                return ScanResultDto.Fail("You are not allowed to scan this QR for another section.", "WRONG_SECTION");
            }

            bool isEnrolled = await _repo.IsStudentEnrolledAsync(studentId, session.SubjectId.Value);
            if (!isEnrolled)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Student is not enrolled.");
                return ScanResultDto.Fail("Student is not enrolled in this subject.", "NOT_ENROLLED");
            }

            // BLOCK MULTIPLE DEVICES
            if (!string.IsNullOrWhiteSpace(registeredDeviceId) &&
                !string.Equals(registeredDeviceId, deviceId, StringComparison.Ordinal))
            {
                await _repo.LogDeviceScanAsync(
                    studentId,
                    sessionId,
                    deviceId,
                    "Suspicious",
                    $"Different device used. Registered device: {registeredDeviceId}, attempted device: {deviceId}"
                );

                return ScanResultDto.Fail(
                    "Suspicious scan detected: this account is already linked to another device.",
                    "SUSPICIOUS_DEVICE"
                );
            }

            // AUTO-BIND FIRST DEVICE
            if (string.IsNullOrWhiteSpace(registeredDeviceId) && !string.IsNullOrWhiteSpace(deviceId))
            {
                await _userRepo.UpdateStudentDeviceAsync(studentId, deviceId);
            }

            var existing = await _repo.GetAttendance(sessionId, studentId);
            if (existing != null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Attendance already recorded.");
                return ScanResultDto.Fail("Attendance already recorded.", "DUPLICATE");
            }

            string status = "Present";

            if (session.GraceTime != null && now > session.GraceTime.Value)
                status = "Late";

            try
            {
                await _repo.InsertAttendance(sessionId, studentId, status, deviceId);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Attendance already recorded.");
                return ScanResultDto.Fail("Attendance already recorded.", "DUPLICATE");
            }

            await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Success", $"Attendance recorded: {status}");

            return ScanResultDto.Ok($"Attendance recorded successfully: {status}", status);
        }

        // TEACHER CREATE SESSION
        public async Task<CreateSessionResponseDto> CreateSession(CreateAttendanceSessionDto dto, int teacherId)
        {
            bool subjectOwned = await _repo.DoesSubjectBelongToTeacherAsync(dto.SubjectId, teacherId);
            if (!subjectOwned)
                throw new Exception("You are not allowed to create a session for this subject.");

            var now = DateTime.Now;

            int graceMinutes = dto.GraceMinutes ?? 5;
            int expirationMinutes = dto.ExpirationMinutes ?? 30;

            if (graceMinutes <= 0)
                graceMinutes = 5;

            if (expirationMinutes <= graceMinutes)
                expirationMinutes = graceMinutes + 5;

            DateTime startTime = now;
            DateTime? graceTime = now.AddMinutes(graceMinutes);
            DateTime expirationTime = now.AddMinutes(expirationMinutes);

            string qrToken = Guid.NewGuid().ToString();

            int sessionId = await _repo.CreateSession(
                dto.Title,
                dto.SubjectId,
                dto.SectionId,
                "",
                expirationTime,
                startTime,
                graceTime,
                teacherId,
                qrToken
            );

            var session = await _repo.GetSession(sessionId);
            if (session == null)
                throw new Exception("Failed to create session.");

            string qrContent = $"{sessionId}|{session.QrToken}";
            string qrCode = QRCodeHelper.GenerateQRCode(qrContent);

            await _repo.UpdateQRCode(sessionId, qrCode);

            return new CreateSessionResponseDto
            {
                SessionId = sessionId,
                QrCode = qrCode
            };
        }

        // TEACHER CLOSE SESSION
        public async Task<OperationResultDto> CloseSession(int sessionId, int teacherId)
        {
            bool success = await _repo.CloseSession(sessionId, teacherId);

            if (!success)
                return OperationResultDto.Fail("Session already closed, not found, or you are not allowed to close it.");

            await _repo.MarkAbsentStudents(sessionId);

            return OperationResultDto.Ok("Session closed successfully.");
        }

        // TEACHER SECTION/SUBJECT
        public async Task<IEnumerable<TeacherSectionSubjectsDto>> GetTeacherSectionSubjectsAsync(int teacherId)
        {
            return await _repo.GetTeacherSectionSubjectsAsync(teacherId);
        }

        // TEACHER GET QR CODE
        public async Task<string?> GetQRCode(int sessionId, int teacherId)
        {
            return await _repo.GetQRCode(sessionId, teacherId);
        }

        public async Task<IEnumerable<dynamic>> GetDeviceLogs(string? status = null)
        {
            return await _repo.GetDeviceLogsAsync(status);
        }

        public async Task<IEnumerable<dynamic>> GetSuspiciousLogs()
        {
            return await _repo.GetSuspiciousLogsAsync();
        }

        private static string? NormalizeDeviceId(string? deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return null;

            return deviceId.Trim().ToLowerInvariant();
        }
    }
}