using QRAttendance.API.Models;

public class AttendanceService
{
    private readonly IAttendanceRepository _repository;

    public AttendanceService(IAttendanceRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateSessionAsync(string title)
        => _repository.CreateSessionAsync(title);

    public Task<IEnumerable<AttendanceSession>> GetAllSessionsAsync()
        => _repository.GetAllSessionsAsync();

    public Task<AttendanceSession?> GetSessionByIdAsync(int id)
        => _repository.GetSessionByIdAsync(id);

    public Task UpdateSessionAsync(int id, string title, bool isActive)
        => _repository.UpdateSessionAsync(id, title, isActive);

    public Task DeleteSessionAsync(int id)
        => _repository.DeleteSessionAsync(id);

    public async Task<MarkAttendanceResult> MarkAttendanceAsync(
    int sessionId,
    int studentId,
    string studentName)
    {
        return await _repository.MarkAttendanceAsync(sessionId, studentId, studentName);
    }

    public Task<IEnumerable<AttendanceRecord>> GetAttendanceBySessionAsync(int id)
        => _repository.GetAttendanceBySessionAsync(id);

    public Task<IEnumerable<AttendanceRecord>> GetStudentAttendanceAsync(int id)
        => _repository.GetStudentAttendanceAsync(id);

    public Task CloseSessionAsync(int id)
        => _repository.CloseSessionAsync(id);

    public Task DeleteAttendanceAsync(int id)
        => _repository.DeleteAttendanceAsync(id);


}