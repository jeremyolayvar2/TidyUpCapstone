namespace TidyUpCapstone.Models.DTOs.API
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? RequestId { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<T> ErrorResult(string message, string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Success(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static new ApiResponse ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static new ApiResponse ErrorResult(string message, string error)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }

    public class ValidationErrorResponse
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Code { get; set; }
        public object? AttemptedValue { get; set; }
    }

    public class ErrorDetails
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Field { get; set; }
        public object? Value { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; } = new Dictionary<string, object>();
    }
}