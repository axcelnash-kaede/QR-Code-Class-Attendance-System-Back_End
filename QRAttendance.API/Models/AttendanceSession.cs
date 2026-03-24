namespace QRAttendance.API.Models;
public class AttendanceSession
{
    public int Id { get; set; }

    public string Subject { get; set; } = string.Empty;

    public int TeacherId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime GraceTime { get; set; }   
    public DateTime ExpirationTime { get; set; }

    public string? QrCode { get; set; }        
    public bool IsClosed { get; set; } 
    public DateTime CreatedAt { get; set; } 
}