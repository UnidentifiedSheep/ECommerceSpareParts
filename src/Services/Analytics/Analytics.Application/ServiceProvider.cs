using Abstractions.Interfaces;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Services;
using Analytics.Application.Services.FactSynchronizers;
using Analytics.Application.Services.Metrics.Calculators;
using Analytics.Application.Services.Metrics.Converters;
using Analytics.Application.Services.Metrics.Validators;
using Analytics.Entities.Metrics;
using Application.Common;
using Application.Common.Interfaces.Currency;
using Application.Common.Services;
using Application.Common.Services.Currency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace Analytics.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        collection
            .AddApplicationBase(configuration, typeof(Global).Assembly)
            .RegisterMetricCalculators()
            .RegisterMetricConverters()
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer();

        collection.AddSingleton<IJsonSerializer, JsonSerializer>();
        collection.AddScoped<ICurrencyConverter, CurrencyConverter>();
        collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();
        collection.AddScoped<IPurchaseFactSynchronizer, PurchaseFactSynchronizer>();

        return collection;
    }

    private static IServiceCollection RegisterMetricCalculators(this IServiceCollection collection)
    {
        collection.AddSingleton<IMetricCalculatorRegistry, MetricCalculatorRegistry>();
        collection.AddScoped<IMetricCalculatorFactory, MetricCalculatorFactory>();
        collection.AddScoped<IMetricValidatorDispatcher, MetricValidatorDispatcher>();

        collection.AddScoped<IMetricCalculator<ArticleSalesMetric>, ArticleSalesMetricCalculator>();
        collection.AddScoped<IMetricCalculator<ArticlePurchasesMetric>, ArticlePurchasesMetricCalculator>();
        return collection;
    }

    private static IServiceCollection RegisterMetricConverters(this IServiceCollection collection)
    {
        collection.AddSingleton<IMetricConverterDispatcher, MetricConverterDispatcher>();
        collection.AddSingleton<IMetricConverter<ArticlePurchasesMetric>, ArticlePurchaseMetricConverter>();
        collection.AddSingleton<IMetricConverter<ArticleSalesMetric>, ArticleSaleMetricConverter>();

        return collection;
    }
}
