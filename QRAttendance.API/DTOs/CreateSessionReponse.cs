namespace QRAttendance.API.DTOs
{
    public class CreateSessionResponse
    {
        public int SessionId { get; set; }
        public string QrCode { get; set; }
    }
}