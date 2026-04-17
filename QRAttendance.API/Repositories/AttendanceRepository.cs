using Dapper;
using QRAttendance.API.Data;
using QRAttendance.API.Models;
using System.Data;

namespace QRAttendance.API.Repositories
{
    public class AttendanceRepository
    {
        private readonly DapperContext _context;

        public AttendanceRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<AttendanceSession?> GetSession(int sessionId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<AttendanceSession>(
                @"SELECT * 
                  FROM dbo.AttendanceSession 
                  WHERE Id = @sessionId",
                new { sessionId });
        }

        public async Task<AttendanceRecord?> GetAttendance(int sessionId, int studentId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<AttendanceRecord>(
                @"SELECT * 
                  FROM dbo.AttendanceRecords
                  WHERE SessionId = @sessionId
                    AND StudentId = @studentId",
                new { sessionId, studentId });
        }

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int subjectId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"SELECT COUNT(1)
                        FROM dbo.Enrollments
                        WHERE StudentId = @studentId
                          AND SubjectId = @subjectId";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { studentId, subjectId });
            return count > 0;
        }

        public async Task<bool> DoesSubjectBelongToTeacherAsync(int subjectId, int teacherId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"SELECT COUNT(1)
                        FROM dbo.Subjects
                        WHERE Id = @subjectId
                          AND TeacherId = @teacherId";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { subjectId, teacherId });
            return count > 0;
        }

        public async Task<int> CreateSession(
            string? title,
            int subjectId,
            int sectionId,
            string qrCode,
            DateTime expirationTime,
            DateTime startTime,
            DateTime? graceTime,
            int teacherId,
            string qrToken)
        {
            var query = @"
                INSERT INTO dbo.AttendanceSession
                (
                    Title,
                    SubjectId,
                    SectionId,
                    QrCode,
                    ExpirationTime,
                    StartTime,
                    GraceTime,
                    TeacherId,
                    QrToken,
                    IsActive,
                    CreatedAt,
                    IsClosed
                )
                VALUES
                (
                    @Title,
                    @SubjectId,
                    @SectionId,
                    @QrCode,
                    @ExpirationTime,
                    @StartTime,
                    @GraceTime,
                    @TeacherId,
                    @QrToken,
                    1,
                    GETDATE(),
                    0
                );

                SELECT CAST(SCOPE_IDENTITY() as int);
            ";

            using var connection = _context.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(query, new
            {
                Title = title,
                SubjectId = subjectId,
                SectionId = sectionId,
                QrCode = qrCode,
                ExpirationTime = expirationTime,
                StartTime = startTime,
                GraceTime = graceTime,
                TeacherId = teacherId,
                QrToken = qrToken
            });
        }

        public async Task InsertAttendance(int sessionId, int studentId, string status, string? deviceId)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO dbo.AttendanceRecords
                  (
                      SessionId,
                      StudentId,
                      DeviceId,
                      Status,
                      ScanTime,
                      IsValid,
                      CreatedAt
                  )
                  VALUES
                  (
                      @sessionId,
                      @studentId,
                      @deviceId,
                      @status,
                      GETDATE(),
                      1,
                      GETDATE()
                  )",
                new { sessionId, studentId, deviceId, status }
            );
        }

        public async Task<bool> CloseSession(int sessionId, int teacherId)
        {
            using var connection = _context.CreateConnection();

            var affectedRows = await connection.ExecuteAsync(
                @"UPDATE dbo.AttendanceSession
                  SET IsActive = 0,
                      IsClosed = 1
                  WHERE Id = @sessionId
                    AND TeacherId = @teacherId",
                new { sessionId, teacherId });

            return affectedRows > 0;
        }

        public async Task UpdateQRCode(int sessionId, string qrCode)
        {
            using var connection = _context.CreateConnection();

            var sql = @"UPDATE dbo.AttendanceSession
                        SET QrCode = @QrCode
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                QrCode = qrCode,
                Id = sessionId
            });
        }

        public async Task MarkAbsentStudents(int sessionId)
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync(
                "dbo.SP_QRAttendanceDB_AutoAbsent",
                new { SessionId = sessionId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<string?> GetQRCode(int sessionId, int teacherId)
        {
            using var connection = _context.CreateConnection();

            return await connection.ExecuteScalarAsync<string?>(
                @"SELECT QrCode
                  FROM dbo.AttendanceSession
                  WHERE Id = @sessionId
                    AND TeacherId = @teacherId",
                new { sessionId, teacherId });
        }

        public async Task<IEnumerable<dynamic>> GetDeviceLogsAsync(string? status = null)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
        SELECT
            l.Id,
            l.StudentId,
            u.FullName AS StudentName,
            l.SessionId,
            l.DeviceId,
            l.AttemptTime,
            l.Status,
            l.Message
        FROM dbo.DeviceScanLogs l
        INNER JOIN dbo.Users u
            ON l.StudentId = u.Id
        WHERE (@Status IS NULL OR l.Status = @Status)
        ORDER BY l.AttemptTime DESC";

            return await connection.QueryAsync(sql, new { Status = status });
        }

        public async Task<IEnumerable<dynamic>> GetSuspiciousLogsAsync()
        {
            using var connection = _context.CreateConnection();

            var sql = @"
        SELECT
            l.Id,
            l.StudentId,
            u.FullName AS StudentName,
            l.SessionId,
            l.DeviceId,
            l.AttemptTime,
            l.Status,
            l.Message
        FROM dbo.DeviceScanLogs l
        INNER JOIN dbo.Users u
            ON l.StudentId = u.Id
        WHERE l.Status = 'Suspicious'
        ORDER BY l.AttemptTime DESC";

            return await connection.QueryAsync(sql);
        }

        // NEW: log every device scan attempt
        public async Task LogDeviceScanAsync(int studentId, int? sessionId, string? deviceId, string status, string? message)
        {
            using var connection = _context.CreateConnection();

            var sql = @"INSERT INTO dbo.DeviceScanLogs
                        (
                            StudentId,
                            SessionId,
                            DeviceId,
                            AttemptTime,
                            Status,
                            Message
                        )
                        VALUES
                        (
                            @StudentId,
                            @SessionId,
                            @DeviceId,
                            GETDATE(),
                            @Status,
                            @Message
                        )";

            await connection.ExecuteAsync(sql, new
            {
                StudentId = studentId,
                SessionId = sessionId,
                DeviceId = deviceId,
                Status = status,    
                Message = message
            });
        }
    }
}