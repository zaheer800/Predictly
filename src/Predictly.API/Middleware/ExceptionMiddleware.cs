using System.Net;
using System.Text.Json;
using Predictly.API.Models;
using Predictly.Domain.Exceptions;

namespace Predictly.API.Middleware;

/// <summary>
/// Global exception handler — no try/catch in controllers (CLAUDE.md §11.1).
/// Maps domain exceptions to appropriate HTTP status codes.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = exception switch
        {
            NotFoundException ex          => (HttpStatusCode.NotFound,           "NOT_FOUND",          ex.Message),
            PredictionLockedException ex  => (HttpStatusCode.Conflict,           "PREDICTION_LOCKED",  ex.Message),
            BusinessRuleException ex      => (HttpStatusCode.UnprocessableEntity,"BUSINESS_RULE_ERROR", ex.Message),
            _                             => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR",    "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(code, message);
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
