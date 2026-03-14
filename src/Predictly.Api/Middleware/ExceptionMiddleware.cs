using System.Text.Json;
using Predictly.Core.Exceptions;

namespace Predictly.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteJsonError(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (PredictionLockedException ex)
        {
            await WriteJsonError(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (Predictly.Core.Exceptions.InvalidOperationException ex)
        {
            await WriteJsonError(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (PredictlyException ex)
        {
            await WriteJsonError(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteJsonError(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteJsonError(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = message }));
    }
}
