using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;

namespace QRAttendance.API.Helpers
{
    public static class QRCodeHelper
    {
        public static string GenerateQRCode(string plainText)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(plainText, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrData))
            using (var bitmap = qrCode.GetGraphic(20))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                var base64 = Convert.ToBase64String(stream.ToArray());
                return $"data:image/png;base64,{base64}";
            }
        }
    }
}