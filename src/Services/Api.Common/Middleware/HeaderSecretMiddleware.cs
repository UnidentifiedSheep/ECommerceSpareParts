using Api.Common.Models;
using Microsoft.Extensions.Options;

namespace Api.Common.Middleware;

public class HeaderSecretMiddleware(IOptionsMonitor<HeaderSecretOptions> optionsMonitor) : IMiddleware
{
    private static readonly PathString MetricsPath = new("/metrics");
    private static readonly PathString HealthPath = new("/health");

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var option = optionsMonitor.CurrentValue;
        if (context.Request.Path.StartsWithSegments(MetricsPath) ||
            context.Request.Path.StartsWithSegments(HealthPath))
        {
            await next(context);
            return;
        }

        var receivedToken = context.Request.Headers["X-Gateway-Token"].FirstOrDefault();

        if (string.IsNullOrEmpty(receivedToken) || receivedToken != option.Key)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Не валидный токен.");
            return;
        }

        await next(context);
    }
}