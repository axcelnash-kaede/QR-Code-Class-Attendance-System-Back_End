namespace QRAttendance.API.DTOs
{
    public class RegisterDto
    {
        public string? StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int SectionId { get; set; }
    }
}