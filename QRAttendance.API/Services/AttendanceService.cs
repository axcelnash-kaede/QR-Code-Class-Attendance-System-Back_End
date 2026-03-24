using System;
using System.Threading.Tasks;
using QRAttendance.API.Models;
using QRAttendance.API.Repositories;
using QRAttendanceAPI.Repositories;

namespace QRAttendance.API.Services;

public class AttendanceService
{
    private readonly AttendanceRepository _repo;

    public AttendanceService(AttendanceRepository repo)
    {
        _repo = repo;
    }

    public async Task<string> ScanQR(int sessionId, int studentId, string deviceId)
    {
        var session = await _repo.GetSession(sessionId);

        if (session == null)
            return "Session not found";

        if (session.IsClosed)
            return "Session closed";

        if (DateTime.Now > session.ExpirationTime)
            return "QR expired";

        var existing = await _repo.GetAttendance(sessionId, studentId);

        if (existing != null)
            return "Already scanned";

        string status;

        var grace = session.StartTime.AddMinutes(10);

        if (DateTime.Now <= grace)
            status = "Present";
        else
            status = "Late";

        await _repo.InsertAttendance(sessionId, studentId, status, deviceId);

        return status;
    }

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
}