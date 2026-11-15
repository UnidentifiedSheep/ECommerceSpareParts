using Application.Common.Host;
using Main.Application.Configs;
using Main.Application.Services.ProductDeletion;
using Main.Modules.Product.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Order.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrderApplication(this IServiceCollection services)
	{
		OrderSortByConfig.Configure();

		services.AddModuleApplication(OrderApplicationAssembly.Value);
		services.AddScoped<IProductDeletionRule, OrderItemProductDeletionRule>();

		return services;
	}
}
