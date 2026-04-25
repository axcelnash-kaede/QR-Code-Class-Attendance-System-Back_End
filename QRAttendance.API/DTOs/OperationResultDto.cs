namespace QRAttendance.API.DTOs
{
    public class OperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static OperationResultDto Ok(string message)
        {
            return new OperationResultDto
            {
                Success = true,
                Message = message
            };
        }

        public static OperationResultDto Fail(string message)
        {
            return new OperationResultDto
            {
                Success = false,
                Message = message
            };
        }
    }
}