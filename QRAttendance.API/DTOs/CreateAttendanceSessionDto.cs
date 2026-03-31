namespace QRAttendance.API.DTOs
{
    public class CreateAttendanceSessionDto
    {
        public string Title { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public int DurationMinutes { get; set; } // for automatic expiration
    }
}