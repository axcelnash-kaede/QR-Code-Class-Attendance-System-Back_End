namespace QRAttendance.API.DTOs
{
    public class DeviceScanLogDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int? SessionId { get; set; }
        public string? DeviceId { get; set; }
        public DateTime AttemptTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}