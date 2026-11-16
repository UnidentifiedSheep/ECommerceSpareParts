using Application.Common.Extensions;
using Application.Common.Host;
using Application.Common.Interfaces;
using Application.Common.NamedObject;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;
using Pricing.Application.Configuration;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PricePolicy;
using Pricing.Application.Services;
using Pricing.Application.Services.Markup;
using Pricing.Application.Services.Pricing;
using Pricing.Application.Services.Pricing.OfferScorers;
using Pricing.Application.Services.Pricing.PricePolicies;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Application.Startup;

namespace Pricing.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPricingApplication(this IServiceCollection services)
	{
		var assembly = typeof(Global).Assembly;
		PricingSortByConfig.Configure();

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly)
			.RegisterNamedObject<SettingDefinitionNamedObjectBase>(assembly)
			.RegisterNamedObject<ApplierNamedObjectBase>(
				assembly,
				ServiceLifetime.Scoped,
				typeof(DynamicApplierNamedObject));

		services.RegisterJobProviders(assembly);

		services.AddScoped<IStartupTask, MarkupInitializationStartupTask>();
		services.AddScoped<IPriceApplierProvider, PriceApplierProvider>();

		services.AddSingleton<IMarkupContainer, MarkupContainer>();
		services.AddScoped<IMarkupCalculator, MarkupCalculator>();
		services.AddScoped<IMarkupInitializer, MarkupInitializer>();
		services.AddScoped<IMarkupRefresher, MarkupRefresher>();
		services.AddScoped<IPriceApplierService, PriceApplierService>();

		services.AddScoped<ISupplierOfferRequestMarkerService, SupplierOfferRequestMarkerService>();
		services.AddScoped<ISupplierOfferExtractorService, SupplierOfferExtractorService>();
		services.AddScoped<ISupplierOfferConverterService, SupplierOfferConverterService>();
		services.AddScoped<IOfferRefreshService, OfferRefreshService>();

		services
			.AddScoped<IOfferScorer, OfferScorerByEffectiveCost>()
			.AddScoped<IInternalPricePolicy, InternalPricePolicy>()
			.AddScoped<ISupplierPricePolicy, SupplierPricePolicy>()
			.AddScoped<IMarketInfoFactory, MarketInfoFactory>()
			.AddScoped<IPriceCandidateBuilder, PriceCandidateBuilder>()
			.AddScoped<IProductPriceCalculator, ProductPriceCalculator>();

		return services;
	}
}
