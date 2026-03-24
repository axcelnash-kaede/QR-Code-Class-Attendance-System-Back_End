namespace QRAttendance.API.Models
{
public class User
{
    public int Id { get; set; }

   // Student inputs this manually (4 digit school ID)
    public string StudentId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    }
}