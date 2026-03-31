using QRAttendance.API.Helpers;
using QRAttendance.API.Models;
using QRAttendance.API.Repositories;
using QRAttendance.API.DTOs;

namespace QRAttendance.API.Services;

public class AttendanceService
{
    private readonly AttendanceRepository _repo;

    public AttendanceService(AttendanceRepository repo)
    {
        _repo = repo;
    }

    // 🔥 SECURE SCAN QR
    public async Task<string> ScanQR(string qrContent, int studentId, string deviceId)
    {
        // 🟢 1. Split QR content
        var parts = qrContent.Split('|');

        if (parts.Length != 2)
            return "Invalid QR format";

        int sessionId = int.Parse(parts[0]);
        string token = parts[1];

        var session = await _repo.GetSession(sessionId);

        if (session == null)
            return "Session not found";

        // 🔐 TOKEN VALIDATION
        if (session.QrToken != token)
            return "Invalid QR";

        if (session.IsClosed)
            return "Session closed";

        if (DateTime.Now > session.ExpirationTime)
            return "QR expired";

        // 🔁 duplicate check
        var existing = await _repo.GetAttendance(sessionId, studentId);

        if (existing != null)
            return "Already scanned";

        // ⏱ late detection
        string status;

        var grace = session.StartTime.AddMinutes(10);

        if (DateTime.Now <= grace)
            status = "Present";
        else
            status = "Late";

        await _repo.InsertAttendance(sessionId, studentId, status, deviceId);

        return status;
    }

    // 🔥 CREATE SESSION (FIXED)
    public async Task<CreateSessionResponse> CreateSession(CreateAttendanceSessionDto dto, int teacherId)
    {
        var qrToken = Guid.NewGuid().ToString();

        var startTime = DateTime.Now;
        var expirationTime = startTime.AddMinutes(dto.DurationMinutes);

        int sessionId = await _repo.CreateSession(
            dto.Title,
            dto.SubjectId,
            null,
            expirationTime,
            startTime,
            teacherId,
            qrToken
        );

        string qrContent = $"{sessionId}|{qrToken}";

        string qrCode = QRCodeHelper.GenerateQRCode(qrContent);

        await _repo.UpdateQRCode(sessionId, qrCode);

        return new CreateSessionResponse
        {
            SessionId = sessionId,
            QrCode = qrCode
        };
    }

    // 🔥 CLOSE SESSION
    public async Task<string> CloseSession(int sessionId)
    {
        var session = await _repo.GetSession(sessionId);

        if (session == null)
            return "Session not found";

        if (session.IsClosed)
            return "Already closed";

        await _repo.CloseSession(sessionId);
        await _repo.MarkAbsentStudents(sessionId);

        return "Session closed";
    }
    public async Task<string?> GetQRCode(int sessionId)
    {
        var session = await _repo.GetSession(sessionId);

        return session?.QrCode;
    }
}