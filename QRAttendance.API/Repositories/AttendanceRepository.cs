using Dapper;
using QRAttendance.API.Data;
using QRAttendance.API.Models;

namespace QRAttendance.API.Repositories
{
    public class AttendanceRepository
    {
        private readonly DapperContext _context;

        public AttendanceRepository(DapperContext context)
        {
            _context = context;
        }

        // GET SESSION
        public async Task<AttendanceSession?> GetSession(int sessionId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<AttendanceSession>(
                "SELECT * FROM AttendanceSession WHERE Id = @sessionId",
                new { sessionId });
        }

        // GET EXISTING ATTENDANCE
        public async Task<AttendanceRecord?> GetAttendance(int sessionId, int studentId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<AttendanceRecord>(
                @"SELECT * FROM AttendanceRecords
                  WHERE SessionId = @sessionId
                  AND StudentId = @studentId",
                new { sessionId, studentId });
        }

        // TEACHER CREATE SESSION
        public async Task<int> CreateSession(string title, string subject, string qrCode, DateTime expirationTime, DateTime startTime, int teacherId)
        {
            var query = @"
        INSERT INTO AttendanceSession
        (Title, Subject, QrCode, ExpirationTime, StartTime, TeacherId, IsActive, CreatedAt, IsClosed)
        VALUES
        (@Title, @Subject, @QrCode, @ExpirationTime, @StartTime, @TeacherId, 1, GETDATE(), 0);
        SELECT CAST(SCOPE_IDENTITY() as int);
    ";

            using var connection = _context.CreateConnection();
            int sessionId = await connection.ExecuteScalarAsync<int>(query, new
            {
                Title = title,
                Subject = subject,
                QrCode = qrCode,
                ExpirationTime = expirationTime,
                StartTime = startTime,
                TeacherId = teacherId
            });

            return sessionId;
        }
        

        // INSERT ATTENDANCE
        public async Task InsertAttendance(int sessionId, int studentId, string status, string deviceId )
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO AttendanceRecords
                (SessionId, StudentId, DeviceId, Status, ScanTime)
                VALUES
                (@sessionId, @studentId, @deviceId, @status, GETDATE())",
                new { sessionId, studentId, deviceId, status });
        }

        // CLOSE SESSION
        public async Task CloseSession(int sessionId)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE AttendanceSession
                  SET IsActive = 0
                  WHERE Id = @sessionId",
                new { sessionId });
        }

        // MARK ABSENT STUDENTS
        public async Task MarkAbsentStudents(int sessionId)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(@"
            INSERT INTO AttendanceRecords (SessionId, StudentId, Status)
            SELECT @sessionId, u.Id, 'Absent'
            FROM Users u
            WHERE u.Role = 'Student'
            AND NOT EXISTS (
                SELECT 1 FROM AttendanceRecords ar
                WHERE ar.SessionId = @sessionId
                AND ar.StudentId = u.Id
            )",
            new { sessionId });
        }
    }
}