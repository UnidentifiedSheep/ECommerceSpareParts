using Application.Common.Host;
using Main.Modules.Logistics.Abstractions;
using Main.Modules.Logistics.Abstractions.Interfaces;
using Main.Modules.Logistics.Application.Cache;
using Main.Modules.Logistics.Application.Configs;
using Main.Modules.Logistics.Application.Interfaces;
using Main.Modules.Logistics.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Logistics.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddLogisticsApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(LogisticsValidationConfiguration.Configure);
		services.AddModuleApplication(LogisticsApplicationAssembly.Value);
		services.AddScoped<IDeliveryScheduleCacheRepository, DeliveryScheduleCacheRepository>();
		services.AddScoped<IDeliveryEstimator, DeliveryEstimator>();
		return services;
	}
}
