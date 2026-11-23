using Application.Common.Host;
using Main.Application.Document.Interfaces.Document;
using Main.Modules.Product.Abstractions;
using Main.Modules.Sale.Abstractions;
using Main.Modules.Sale.Abstractions.Interfaces;
using Main.Modules.Sale.Application.Configs;
using Main.Modules.Sale.Application.Interfaces.Services;
using Main.Modules.Sale.Application.Services;
using Main.Modules.Sale.Application.Services.ProductDeletion;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

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
		services.RegisterNamedObject<IDocumentDefinition>(
			SaleApplicationAssembly.Value,
			objectsLifetime: ServiceLifetime.Scoped);

		return services;
	}
}
