using Application.Common.Host;
using Application.Common.Interfaces.Lrt;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services;
using Main.Application.Products.Interfaces;
using Main.Application.Products.Services;
using Main.Application.Products.Services.EventParts;
using Main.Application.Services;
using Main.Cache;
using Main.Modules.Product.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Product.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddProductApplication(this IServiceCollection services)
	{
		var assembly = ProductApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly);

		services.AddScoped<IProductEventPublisher, ProductEventPublisher>();
		services.AddScoped<IProductEventProvider, ProductEventProvider>();
		services.AddScoped<IProductEventPartProvider, ProductCoreEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductCharacteristicsEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductDimensionsEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductWeightEventPartProvider>();
		services.AddScoped<IProductEventPartProvider, ProductStocksEventPartProvider>();

		services.AddSingleton<IProducerLookupService, ProducerLookupService>();
		services.AddScoped<IProducerLookupLoader, ProducerLookupLoader>();

		services.AddScoped<IProductReferenceResolver, ProductReferenceResolver>();
		services.AddScoped<IProductProvider, ProductProvider>();
		services.AddScoped<IProductCacheInvalidator, ProductCacheInvalidator>();

		return services;
	}
}
