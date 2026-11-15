using Application.Common.Host;
using Main.Application.Configs;
using Main.Application.Services.ProductDeletion;
using Main.Modules.Product.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Purchase.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPurchaseApplication(this IServiceCollection services)
	{
		PurchaseSortByConfig.Configure();

		services.AddModuleApplication(PurchaseApplicationAssembly.Value);
		services.AddScoped<IProductDeletionRule, PurchaseContentProductDeletionRule>();

		return services;
	}
}
