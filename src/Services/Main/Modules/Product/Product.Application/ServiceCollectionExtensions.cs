using Application.Common.Extensions;
using Application.Common.Host;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Lrt;
using Main.Modules.Product.Abstractions;
using Main.Modules.Product.Application.Cache;
using Main.Modules.Product.Application.Configs;
using Main.Modules.Product.Application.Interfaces.Cache;
using Main.Modules.Product.Application.Interfaces.Services;
using Main.Modules.Product.Application.Products.Interfaces;
using Main.Modules.Product.Application.Products.Services;
using Main.Modules.Product.Application.Products.Services.EventParts;
using Main.Modules.Product.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Product.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddProductApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(ProductValidationConfiguration.Configure);
		ProductSortByConfig.Configure();

		var assembly = ProductApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly)
			.RegisterJobProviders(assembly);

		services.AddScoped<IProductCrosses, ProductCrosses>();
		services.AddScoped<IProductSearch, ProductSearch>();
		services.AddScoped<IProductNames, ProductNames>();
		services.AddScoped<IProductEventPublisher, ProductEventPublisher>();
		services.AddScoped<IProductEventProvider, ProductEventProvider>();
		services.AddScoped<IProductEventPartProvider, ProductCoreEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductCharacteristicsEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductDimensionsEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductWeightEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductStocksEventPartProvider>();

		services.AddSingleton<IProducerLookupService, ProducerLookupService>();
		services.AddScoped<IProducerLookupLoader, ProducerLookupLoader>();

		services.AddScoped<IProductChecks, ProductChecks>();
		services.AddScoped<IProductStockWriter, ProductStockWriter>();
		services.AddScoped<IProductReferenceResolver, ProductReferenceResolver>();
		services.AddScoped<IProductDeletionBlockerResolver, ProductDeletionBlockerResolver>();
		services.AddScoped<IProductDeletionRule, ProductStockDeletionRule>();
		services.AddScoped<IProductDeletionRule, ProductCompositionDeletionRule>();
		services.AddScoped<IProductProvider, ProductProvider>();
		services.AddScoped<IProductCacheInvalidator, ProductCacheInvalidator>();

		return services;
	}
}
