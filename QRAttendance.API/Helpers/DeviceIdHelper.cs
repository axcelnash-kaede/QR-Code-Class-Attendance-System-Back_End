using System.Security.Cryptography;
using System.Text;

namespace QRAttendance.API.Helpers
{
    public static class DeviceIdHelper
    {
        public static string GenerateStableDeviceId(int studentId, string? userAgent)
        {
            string raw = $"{studentId}|{userAgent}|QRAttendanceDevice";

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));

            return Convert.ToHexString(hashBytes);
        }
    }
}