namespace CareNest_SePay.Application.Common
{
    public class ResponseResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ResponseResult<T> Success(T data, string? message = null)
        {
            return new ResponseResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static ResponseResult<T> Failure(string message, List<string>? errors = null)
        {
            return new ResponseResult<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
