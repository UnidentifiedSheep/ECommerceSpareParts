using Main.Application.Analytics.Charts;
using Main.Application.Analytics.Analyzers;
using Main.Application.Analytics;
using NamedObject.Core.Interfaces;
using Application.Common.Interfaces.Lrt;
using NamedObject;
using Application.Common.Host;
using Main.Application.Analytics.Configs;
using Main.Application.Analytics.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Analytics.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAnalyticsApplication(this IServiceCollection services)
	{
		services.AddSingleton<INamedObjectGroupRegistry, AnalyticsNamedObjectGroupRegistry>();
		services.RegisterNamedObject<MarkupAnalyzerNamedObjectBase>(AnalyticsApplicationAssembly.Value);
		services.RegisterNamedObject<ChartDataSourceNamedObject>(AnalyticsApplicationAssembly.Value);

		AnalyticsCursorConfig.Configure();

		services
			.AddModuleApplication(AnalyticsApplicationAssembly.Value)
			.RegisterNamedObject<ILrtNamedObject>(AnalyticsApplicationAssembly.Value);
		services.AddScoped<SaleContentAnalyticsCalculator>();
		services.AddScoped<SaleAnalyticsWriter>();

		return services;
	}
}
