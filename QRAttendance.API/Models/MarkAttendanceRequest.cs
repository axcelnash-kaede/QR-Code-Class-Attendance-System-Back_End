namespace QRAttendance.API.Models.Requests
{
    public class MarkAttendanceRequest
    {
        public int SessionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
    }
}