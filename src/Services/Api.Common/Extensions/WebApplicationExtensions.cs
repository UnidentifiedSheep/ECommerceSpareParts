using Api.Common.Middleware;
using Carter;
using Localization.Domain.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;

namespace Api.Common.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCommonApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<HeaderSecretMiddleware>();
        app.UseMiddleware<ScopedLocalizationMiddleware>();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseExceptionHandler(_ => { });
        app.UseRouting();
        app.UseCors();
        app.MapCarter();
        app.MapHealthChecks("/health");

        return app;
    }
}
