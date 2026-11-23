using Application.Common.Host;
using Main.Modules.Logistics.Application.Configs;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Logistics.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddLogisticsApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(LogisticsValidationConfiguration.Configure);
		services.AddModuleApplication(LogisticsApplicationAssembly.Value);
		return services;
	}
}
