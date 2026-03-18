namespace QRAttendance.API.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int StudentId { get; set; }   
        public string StudentName { get; set; } = string.Empty;
        public DateTime TimeIn { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SessionOrder { get; set; }
    }
}