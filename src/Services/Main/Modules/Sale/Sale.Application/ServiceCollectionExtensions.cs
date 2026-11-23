using Application.Common.Host;
using Main.Modules.Product.Abstractions;
using Main.Modules.Sale.Abstractions;
using Main.Modules.Sale.Abstractions.Interfaces;
using Main.Modules.Sale.Application.Configs;
using Main.Modules.Sale.Application.Interfaces.Services;
using Main.Modules.Sale.Application.Services;
using Main.Modules.Sale.Application.Services.ProductDeletion;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Sale.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSaleApplication(this IServiceCollection services)
	{
		SaleSortByConfig.Configure();

		services.AddModuleApplication(SaleApplicationAssembly.Value);
		services.AddScoped<ISaleService, SaleService>();
		services.AddScoped<ISaleBuyCostCalculator, SaleBuyCostCalculator>();
		services.AddScoped<ISaleSnapshots, SaleSnapshots>();
		services.AddScoped<ISaleContentSource, SaleContentSource>();
		services.AddScoped<ISalesFilterApplier, SalesFilterApplier>();
		services.AddScoped<IProductDeletionRule, SaleContentProductDeletionRule>();

		return services;
	}
}
