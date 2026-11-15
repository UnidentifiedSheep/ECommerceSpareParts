using Application.Common.Host;
using Main.Application.Configs;
using Main.Application.Interfaces.Services;
using Main.Application.Services;
using Main.Application.Services.ProductDeletion;
using Main.Modules.Product.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Sale.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSaleApplication(this IServiceCollection services)
	{
		SaleSortByConfig.Configure();

		services.AddModuleApplication(SaleApplicationAssembly.Value);
		services.AddScoped<ISaleService, SaleService>();
		services.AddScoped<IProductDeletionRule, SaleContentProductDeletionRule>();

		return services;
	}
}
