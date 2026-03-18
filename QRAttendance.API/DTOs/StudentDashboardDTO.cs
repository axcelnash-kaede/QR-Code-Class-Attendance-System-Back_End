namespace QRAttendance.API.DTOs
{
    public class StudentDashboardDto
    {
        public int TotalSessions { get; set; }

        public int SessionsAttended { get; set; }

        public int SessionsMissed { get; set; }

        public double AttendancePercentage { get; set; }
    }
}