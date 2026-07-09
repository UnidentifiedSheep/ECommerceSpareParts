using Abstractions;
using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces.Currency;
using Application.Common.Services.Currency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Configuration;
using Pricing.Application.Interfaces;
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
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;
using ZiggyCreatures.Caching.Fusion;

namespace Pricing.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        SortByConfig.Configure();
        collection.AddApplicationBase(
                ServicesDefinitions.Pricing,
                configuration,
                typeof(Global).Assembly)
            .AddNamedObjects()
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer();

        collection.RegisterSettingsService<SettingFactory>();

        collection.AddScoped<ICurrencyConverter, CurrencyConverter>();
        collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();

        collection.AddSingleton<IMarkupContainer, MarkupContainer>();
        collection.AddScoped<IMarkupCalculator, MarkupCalculator>();
        collection.AddScoped<IMarkupInitializer, MarkupInitializer>();

        collection.AddScoped<ISupplierOfferExtractorService, SupplierOfferExtractorService>();
        collection.AddScoped<ISupplierOfferConverterService, SupplierOfferConverterService>();

        collection.AddPricingServices();
        
        return collection;
    }

    private static IServiceCollection AddPricingServices(this IServiceCollection collection)
    {
        collection
            .AddPriceAppliers()
            .AddScoped<IOfferScorer, OfferScorerByEffectiveCost>()
            .AddScoped<IInternalPricePolicy, InternalPricePolicy>()
            .AddScoped<ISupplierPricePolicy, SupplierPricePolicy>()
            .AddScoped<IMarketInfoFactory, MarketInfoFactory>()
            .AddScoped<IPriceCandidateBuilder, PriceCandidateBuilder>()
            .AddScoped<IProductPriceCalculator, ProductPriceCalculator>();
        
        return collection;
    }

    private static IServiceCollection AddPriceAppliers(this IServiceCollection collection)
    {
        collection.AddScoped<IInternalPriceApplier, MinimumSupplierPriceApplier>();
        
        collection.AddScoped<IInternalPriceApplier, MarkupApplier>();
        collection.AddScoped<IInternalPriceApplier, PriceRoundingApplier>();
        collection.AddScoped<IInternalPriceApplier, UniquenessAdditionalMarkupApplier>();
        
        collection.AddScoped<ISupplierPriceApplier, MarkupApplier>();
        collection.AddScoped<ISupplierPriceApplier, PriceRoundingApplier>();
        return collection;
    }
}