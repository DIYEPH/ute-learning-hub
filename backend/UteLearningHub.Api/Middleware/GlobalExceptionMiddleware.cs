using System.Net;
using System.Text.Json;
using UteLearningHub.Domain.Exceptions.Base;

namespace UteLearningHub.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
            await HandleExceptionAsync(context, ex);
        }
    }
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case IAppException appException:
                response.StatusCode = appException.StatusCode;
                response.Title = appException.Title ?? "Application Error";
                response.Message = exception.Message ?? "An error occurred";
                response.Type = appException.Type;
                break;

            case Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Title = "Conflict";
                response.Message = "The resource has been modified by another user. Please reload and try again.";
                response.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                response.Message = exception.Message;
                response.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Title = "Internal Server Error";
                response.Message = "An unexpected error occurred";
                response.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
                break;
        }
        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
}