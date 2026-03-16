namespace Predictly.API.Models;

/// <summary>
/// Standard response envelope for all API responses.
/// Success: { success: true, data: {...}, error: null }
/// Failure: { success: false, data: null, error: { code, message } }
/// </summary>
public record ApiResponse<T>(bool Success, T? Data, ApiError? Error)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null);
    public static ApiResponse<T> Fail(string code, string message) => new(false, default, new ApiError(code, message));
}

public record ApiError(string Code, string Message);
