using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Common.Extensions;

public static class CorsServiceCollectionExtensions
{
	public static IServiceCollection AddConfiguredCors(this IServiceCollection services)
	{
		services.AddCors();
		services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
		{
			var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
			options.AddDefaultPolicy(policy => policy
				.WithOrigins(origins)
				.SetIsOriginAllowedToAllowWildcardSubdomains()
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials());
		});

		return services;
	}
}
