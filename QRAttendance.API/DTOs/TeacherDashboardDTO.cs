namespace QRAttendance.API.DTOs;

public class TeacherDashboardDto
{
    public int TotalSessions { get; set; }

    public int ActiveSessions { get; set; }

    public int ClosedSessions { get; set; }

    public int TotalAttendanceRecords { get; set; }

    public int TotalStudents { get; set; }
}