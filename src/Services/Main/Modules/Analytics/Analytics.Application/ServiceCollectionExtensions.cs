using Application.Common.Host;
using Application.Common.Interfaces;
using Main.Modules.Analytics.Abstractions.Analyzers;
using Main.Modules.Analytics.Abstractions.Charts;
using Main.Modules.Analytics.Abstractions.Interfaces;
using Main.Modules.Analytics.Application.Analyzers.AbcXyz;
using Main.Modules.Analytics.Application.Configs;
using Main.Modules.Analytics.Application.Interfaces;
using Main.Modules.Analytics.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;
using NamedObject.Core.Interfaces;

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
		services.AddScoped<AbcXyzDataReader>();
		services.AddScoped<IAbcXyzAnalyzer, AbcXyzAnalyzer>();
		services.AddSingleton<IAbcXyzCalculator, AbcXyzCalculator>();

		return services;
	}
}
