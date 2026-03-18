namespace QRAttendance.API.Models
{
    public class AttendanceSession
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime StartTime { get; set; } 
        public DateTime EndTime { get; set; }

        // Navigation Property
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; }
            = new List<AttendanceRecord>();
    }
}