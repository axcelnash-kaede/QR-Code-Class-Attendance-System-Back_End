namespace QRAttendance.API.Models
{
    public class AttendanceSession
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool? IsClosed { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? QrCode { get; set; }
        public DateTime? GraceTime { get; set; }
        public string? Title { get; set; }
        public bool IsActive { get; set; }
        public string? QrToken { get; set; }
        public int? SubjectId { get; set; }
        public int? SectionId { get; set; }
    }
}