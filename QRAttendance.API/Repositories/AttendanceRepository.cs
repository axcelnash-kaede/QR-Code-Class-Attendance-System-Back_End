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

        // CREATE SESSION 
        public async Task<int> CreateSession(
            string title,
            int subjectId,
            string qrCode,
            DateTime expirationTime,
            DateTime startTime,
            int teacherId,
            string qrToken)
        {
            var query = @"
            INSERT INTO AttendanceSession
            (Title, SubjectId, QrCode, ExpirationTime, StartTime, TeacherId, QrToken, IsActive, CreatedAt, IsClosed)
            VALUES
            (@Title, @SubjectId, @QrCode, @ExpirationTime, @StartTime, @TeacherId, @QrToken, 1, GETDATE(), 0);

            SELECT CAST(SCOPE_IDENTITY() as int);
            ";

            using var connection = _context.CreateConnection();

            int sessionId = await connection.ExecuteScalarAsync<int>(query, new
            {
                Title = title,
                SubjectId = subjectId,
                QrCode = qrCode,
                ExpirationTime = expirationTime,
                StartTime = startTime,
                TeacherId = teacherId,
                QrToken = qrToken // ✅ FIXED
            });

            return sessionId;
        }

        // INSERT ATTENDANCE 
        public async Task InsertAttendance(int sessionId, int studentId, string status, string deviceId)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO AttendanceRecords
                (SessionId, StudentId, DeviceId, Status, ScanTime, IsValid)
                VALUES
                (@sessionId, @studentId, @deviceId, @status, GETDATE(), 1)",
                new { sessionId, studentId, deviceId, status } 
            );
        }

        // CLOSE SESSION
        public async Task CloseSession(int sessionId)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE AttendanceSession
                  SET IsActive = 0,
                      IsClosed = 1
                  WHERE Id = @sessionId",
                new { sessionId });
        }

        // UPDATE QR CODE
        public async Task UpdateQRCode(int sessionId, string qrCode)
        {
            using var connection = _context.CreateConnection();

            var sql = @"UPDATE AttendanceSession 
                        SET QrCode = @QrCode 
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                QrCode = qrCode,
                Id = sessionId
            });
        }

        // MARK ABSENT STUDENTS
        public async Task MarkAbsentStudents(int sessionId)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(@"
            INSERT INTO AttendanceRecords 
            (SessionId, StudentId, Status, ScanTime, IsValid)
            SELECT @sessionId, u.Id, 'Absent', GETDATE(), 1
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