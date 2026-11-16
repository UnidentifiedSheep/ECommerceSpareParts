using Application.Common.Host;
using Main.Modules.Product.Abstractions;
using Main.Modules.Purchase.Application.Configs;
using Main.Modules.Purchase.Application.Services.ProductDeletion;
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
