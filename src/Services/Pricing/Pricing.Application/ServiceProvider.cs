using Abstractions;
using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces.Currency;
using Application.Common.Services.Currency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Services;
using Pricing.Application.Services.Markup;
using Pricing.Application.Services.Pricing;
using ZiggyCreatures.Caching.Fusion;

namespace Pricing.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
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

        return collection;
    }
}