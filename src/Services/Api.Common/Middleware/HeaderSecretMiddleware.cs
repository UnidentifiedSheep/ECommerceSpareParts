namespace Api.Common.Middleware;

public class HeaderSecretMiddleware(string secret) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var receivedToken = context.Request.Headers["X-Gateway-Token"].FirstOrDefault();

        if (string.IsNullOrEmpty(receivedToken) || receivedToken != secret)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Не валидный токен.");
            return;
        }

        await next(context);
    }
}