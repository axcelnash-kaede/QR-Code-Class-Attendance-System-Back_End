namespace QRAttendance.API.DTOs
{
    public class ScanResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Status { get; set; }

        public static ScanResultDto Ok(string message, string status)
        {
            return new ScanResultDto
            {
                Success = true,
                Message = message,
                Code = "SUCCESS",
                Status = status
            };
        }

        public static ScanResultDto Fail(string message, string code)
        {
            return new ScanResultDto
            {
                Success = false,
                Message = message,
                Code = code,
                Status = null
            };
        }
    }
}