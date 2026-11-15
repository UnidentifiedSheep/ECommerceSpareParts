using Application.Common.Host;
using Main.Application.Analytics.Configs;
using Main.Application.Analytics.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Analytics.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAnalyticsApplication(this IServiceCollection services)
	{
		AnalyticsCursorConfig.Configure();

		services.AddModuleApplication(AnalyticsApplicationAssembly.Value);
		services.AddScoped<SaleContentAnalyticsCalculator>();
		services.AddScoped<SaleAnalyticsWriter>();

		return services;
	}
}
