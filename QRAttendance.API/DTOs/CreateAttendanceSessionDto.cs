namespace QRAttendance.API.DTOs
{
    public class CreateAttendanceSessionDto
    {
        public string Title { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } // for automatic expiration
    }
}