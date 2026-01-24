namespace Common.Commons
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public ErrorCode? ErrorCode { get; set; }

        public static ApiResponse SuccessResult(
            int statusCode = HttpStatusCode.Ok,
            string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                StatusCode = statusCode,
                Message = message
            };
        }

        public static ApiResponse ErrorResult(
            string message,
            int statusCode,
            ErrorCode errorCode = Commons.ErrorCode.UnDefinedError)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResult(T data, int statusCode = HttpStatusCode.Ok, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
