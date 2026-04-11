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
        public async Task<string> ScanQR(string qrContent, int studentId, string? deviceId, string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(qrContent))
            {
                await _repo.LogDeviceScanAsync(studentId, null, deviceId, "Blocked", "QR content is required.");
                return "QR content is required.";
            }

            // BETTER DEVICE ID GENERATION
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = DeviceIdHelper.GenerateStableDeviceId(studentId, userAgent);
            }

            var parts = qrContent.Split('|');

            if (parts.Length != 2)
            {
                await _repo.LogDeviceScanAsync(studentId, null, deviceId, "Blocked", "Invalid QR format.");
                return "Invalid QR format.";
            }

            if (!int.TryParse(parts[0], out int sessionId))
            {
                await _repo.LogDeviceScanAsync(studentId, null, deviceId, "Blocked", "Invalid QR session ID.");
                return "Invalid session ID.";
            }

            string qrToken = parts[1];

            if (string.IsNullOrWhiteSpace(qrToken))
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Invalid QR token.");
                return "Invalid QR token.";
            }

            var session = await _repo.GetSession(sessionId);

            if (session == null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session not found.");
                return "Session not found";
            }

            if (session.IsClosed == true)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session is already closed.");
                return "Session is already closed";
            }

            if (!session.IsActive)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session is not active.");
                return "Session is not active";
            }

            var now = DateTime.Now;

            if (now > session.ExpirationTime)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session has expired.");
                return "Session has expired";
            }

            if (session.QrToken != qrToken)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Invalid QR token.");
                return "Invalid QR";
            }

            if (session.SubjectId == null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Session subject is missing.");
                return "Session subject missing";
            }

            bool isEnrolled = await _repo.IsStudentEnrolledAsync(studentId, session.SubjectId.Value);
            if (!isEnrolled)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Student is not enrolled.");
                return "Not enrolled";
            }

            var student = await _userRepo.GetByIdAsync(studentId);
            if (student == null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Student not found.");
                return "Student not found";
            }

            // BLOCK MULTIPLE DEVICES
            if (!string.IsNullOrWhiteSpace(student.DeviceId) && student.DeviceId != deviceId)
            {
                await _repo.LogDeviceScanAsync(
                    studentId,
                    sessionId,
                    deviceId,
                    "Suspicious",
                    $"Different device used. Registered device: {student.DeviceId}, attempted device: {deviceId}"
                );

                return "Suspicious scan detected: this account is already linked to another device.";
            }

            // AUTO-BIND FIRST DEVICE
            if (string.IsNullOrWhiteSpace(student.DeviceId))
            {
                await _userRepo.UpdateStudentDeviceAsync(studentId, deviceId);
            }

            var existing = await _repo.GetAttendance(sessionId, studentId);
            if (existing != null)
            {
                await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Blocked", "Attendance already recorded.");
                return "Already recorded";
            }

            string status = "Present";

            if (session.GraceTime != null && now > session.GraceTime.Value)
                status = "Late";

            await _repo.InsertAttendance(sessionId, studentId, status, deviceId);

            await _repo.LogDeviceScanAsync(studentId, sessionId, deviceId, "Success", $"Attendance recorded: {status}");

            return $"Attendance recorded successfully: {status}";
        }

        // TEACHER CREATE SESSION
        public async Task<CreateSessionResponseDto> CreateSession(CreateAttendanceSessionDto dto, int teacherId)
        {
            bool subjectOwned = await _repo.DoesSubjectBelongToTeacherAsync(dto.SubjectId, teacherId);
            if (!subjectOwned)
                throw new Exception("You are not allowed to create a session for this subject.");

            // AUTO COMPUTE TIME VALUES
            var now = DateTime.Now;

            int graceMinutes = dto.GraceMinutes ?? 5;
            int expirationMinutes = dto.ExpirationMinutes ?? 20;

            if (graceMinutes <= 0)
                graceMinutes = 5;

            if (expirationMinutes <= graceMinutes)
                expirationMinutes = graceMinutes + 5;

            DateTime startTime = now;
            DateTime? graceTime = now.AddMinutes(graceMinutes);
            DateTime expirationTime = now.AddMinutes(expirationMinutes);

            int sessionId = await _repo.CreateSession(
                dto.Title,
                dto.SubjectId,
                "",
                expirationTime,
                startTime,
                graceTime,
                teacherId,
                Guid.NewGuid().ToString()
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
        public async Task<string> CloseSession(int sessionId, int teacherId)
        {
            bool success = await _repo.CloseSession(sessionId, teacherId);

            if (!success)
                return "Session not found or you are not allowed to close it.";

            await _repo.MarkAbsentStudents(sessionId);

            return "Session closed successfully";
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
    }

}