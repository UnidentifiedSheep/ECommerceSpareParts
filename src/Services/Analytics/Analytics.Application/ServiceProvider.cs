using Abstractions.Interfaces;
using Analytics.Application.Configs;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Services;
using Analytics.Application.Services.FactSynchronizers;
using Analytics.Application.Services.Metrics.Calculators;
using Analytics.Application.Services.Metrics.Converters;
using Analytics.Application.Services.Metrics.Validators;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using Application.Common;
using Application.Common.Extensions;
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
        IConfiguration? configuration)
    {
        SortByConfig.Configure();
        collection
            .AddApplicationBase(configuration, typeof(Global).Assembly)
            .RegisterMetricCalculators()
            .RegisterMetricConverters()
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer();
        
        collection.RegisterSettingsService<SettingFactory>();

        collection.AddSingleton<IJsonSerializer, JsonSerializer>();
        collection.AddScoped<ICurrencyConverter, CurrencyConverter>();
        collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();
        collection.AddScoped<IFactSynchronizer<PurchasesFact, Guid>, PurchaseFactSynchronizer>();

        collection.AddScoped<ITagsService, TagsService>();
        return collection;
    }

    private static IServiceCollection RegisterMetricCalculators(this IServiceCollection collection)
    {
        collection.AddSingleton<IMetricCalculatorRegistry, MetricCalculatorRegistry>();
        collection.AddScoped<IMetricCalculatorFactory, MetricCalculatorFactory>();
        collection.AddScoped<IMetricValidatorDispatcher, MetricValidatorDispatcher>();

        collection.AddScoped<IMetricCalculator<ProductSalesMetric>, ProductSalesMetricCalculator>();
        collection.AddScoped<IMetricCalculator<ProductPurchasesMetric>, ProductPurchasesMetricCalculator>();
        return collection;
    }

    private static IServiceCollection RegisterMetricConverters(this IServiceCollection collection)
    {
        collection.AddSingleton<IMetricConverterDispatcher, MetricConverterDispatcher>();
        collection.AddSingleton<IMetricConverter<ProductPurchasesMetric>, ProductPurchaseMetricConverter>();
        collection.AddSingleton<IMetricConverter<ProductSalesMetric>, ProductSaleMetricConverter>();

        return collection;
    }
}
