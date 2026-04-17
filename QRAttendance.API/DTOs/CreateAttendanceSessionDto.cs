namespace QRAttendance.API.DTOs
{
    public class CreateAttendanceSessionDto
    {
        public string? Title { get; set; }
        public int SubjectId { get; set; }
        public int SectionId { get; set; }
        public int? GraceMinutes { get; set; }
        public int? ExpirationMinutes { get; set; }
    }
}