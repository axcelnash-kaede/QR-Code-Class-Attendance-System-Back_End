namespace QRAttendance.API.DTOs
{
    public class CreateSessionResponseDto
    {
        public int SessionId { get; set; }
        public string QrCode { get; set; } = string.Empty;
    }
}