using Api.Common.Middleware;
using Carter;
using Locan.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;

namespace Api.Common.Extensions;

public static class WebApplicationExtensions
{
	public static WebApplication UseCommonApiPipeline(this WebApplication app)
	{
		app.UseMiddleware<HeaderSecretMiddleware>();

		app.UseForwardedHeaders(
			new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

		app.UseLocanRequestLocalization();
		app.UseExceptionHandler(_ =>
		{
		});
		app.UseRouting();
		app.UseCors();

		app.UseAuthentication();
		app.UseAuthorization();
		app.MapCarter();
		app.UseSwagger();
		app.MapHealthChecks("/health");
		app.UseOpenTelemetryPrometheusScrapingEndpoint();

		return app;
	}
}
