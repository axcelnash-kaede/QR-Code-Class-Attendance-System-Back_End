namespace QRAttendance.API.Models;

public class AttendanceRecord
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int StudentId { get; set; }

    public DateTime ScanTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;
}