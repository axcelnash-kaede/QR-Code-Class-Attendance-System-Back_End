using QRAttendance.API.Models;

public interface IAttendanceRepository
{
    Task<int> CreateSessionAsync(string title);
    Task<IEnumerable<AttendanceSession>> GetAllSessionsAsync();
    Task<AttendanceSession?> GetSessionByIdAsync(int sessionId);
    Task UpdateSessionAsync(int id, string title, bool isActive);
    Task DeleteSessionAsync(int sessionId);

    Task<MarkAttendanceResult> MarkAttendanceAsync(int sessionId, int studentId, string studentName);
    Task<IEnumerable<AttendanceRecord>> GetAttendanceBySessionAsync(int sessionId);
    Task<IEnumerable<AttendanceRecord>> GetStudentAttendanceAsync(int studentId);
    Task CloseSessionAsync(int sessionId);
    Task DeleteAttendanceAsync(int attendanceId);
}