using System.Net;
using System.Text.Json;
using SportMap.Application.DTOs.Common;

namespace SportMap.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = ex.Message switch
        {
            var m when m.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => (int)HttpStatusCode.NotFound,

            var m when m.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
                => (int)HttpStatusCode.Forbidden,

            var m when m.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                => (int)HttpStatusCode.Conflict,

            _ => (int)HttpStatusCode.BadRequest
        };

        // دلوقتي بنستخدم ApiResponse بدل Anonymous Object
        var response = ApiResponse<object>.Fail(ex.Message);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}