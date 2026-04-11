namespace QRAttendance.API.DTOs
{
    public class CreateAttendanceSessionDto
    {
        public string Title { get; set; } = string.Empty;
        public int SubjectId { get; set; }

        // Optional custom values
        public int? GraceMinutes { get; set; }
        public int? ExpirationMinutes { get; set; }
    }
}