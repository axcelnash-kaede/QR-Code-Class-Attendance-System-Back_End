namespace QRAttendance.API.Models;

public class MarkAttendanceResult
{
    public string Result { get; set; } = "";
    public string AttendanceStatus { get; set; } = "";
    public int SessionOrder { get; set; }
}