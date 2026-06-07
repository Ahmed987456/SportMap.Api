using SportMap.API.Middleware;

namespace SportMap.API.Extensions;

public static class MiddlewareExtensions
{
    // Extension Method بتخلي الكود في Program.cs أنظف
    // بدل: app.UseMiddleware<ExceptionMiddleware>()
    // بنكتب: app.UseExceptionMiddleware()
    public static IApplicationBuilder UseExceptionMiddleware(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}