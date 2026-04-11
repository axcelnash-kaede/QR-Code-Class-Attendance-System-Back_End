namespace QRAttendance.API.Models
{
    public class DeviceScanLog
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int? SessionId { get; set; }
        public string? DeviceId { get; set; }
        public DateTime AttemptTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}