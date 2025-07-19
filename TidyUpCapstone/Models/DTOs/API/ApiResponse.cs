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

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message, T? data = default, List<string>? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
            Timestamp = DateTime.UtcNow;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>(true, message, data);
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>(false, message, default, errors);
        }

        public static ApiResponse<T> ErrorResponse(string message, string error)
        {
            return new ApiResponse<T>(false, message, default, new List<string> { error });
        }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? RequestId { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message, object? data = null, List<string>? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
            Timestamp = DateTime.UtcNow;
        }

        public static ApiResponse SuccessResponse(string message = "Success")
        {
            return new ApiResponse(true, message);
        }

        public static ApiResponse SuccessResponse(object? data, string message = "Success")
        {
            return new ApiResponse(true, message, data);
        }

        public static ApiResponse ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse(false, message, null, errors);
        }

        public static ApiResponse ErrorResponse(string message, string error)
        {
            return new ApiResponse(false, message, null, new List<string> { error });
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