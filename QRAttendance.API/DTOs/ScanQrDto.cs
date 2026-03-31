namespace QRAttendance.API.DTOs;

public class ScanQrDto
{
    public string QrContent { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;
}