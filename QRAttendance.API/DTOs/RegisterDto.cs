namespace QRAttendance.API.DTOs;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty; // 4-digit input
}
