namespace CareNest_SePay.Application.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public object? Details { get; }

        public BusinessException(string message, string errorCode = "BUSINESS_ERROR", object? details = null) 
            : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public BusinessException(string message, Exception innerException, string errorCode = "BUSINESS_ERROR", object? details = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Details = details;
        }
    }
}
