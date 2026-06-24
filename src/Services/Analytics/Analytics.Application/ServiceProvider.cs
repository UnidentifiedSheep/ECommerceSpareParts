using Abstractions.Interfaces;
using Analytics.Application.Configs;
using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Lrts.MetricCalculation;
using Analytics.Application.Services;
using Analytics.Application.Services.FactSynchronizers;
using Analytics.Application.Services.Metrics.Calculators;
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
            .AddApplicationBase(configuration, typeof(TagsService).Assembly)
            .AddNamedObjects()
            .AddLrtLayer(typeof(MetricCalculationLrt).Assembly)
            .RegisterMetricCalculators()
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer();
        
        collection.RegisterSettingsService<SettingFactory>();

        collection.AddSingleton<IJsonSerializer, JsonSerializer>();
        collection.AddScoped<ICurrencyConverter, CurrencyConverter>();
        collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();
        collection.AddScoped<IFactSynchronizer<PurchasesFact, Guid>, PurchaseFactSynchronizer>();
        collection.AddScoped<IFactSynchronizer<SalesFact, Guid>, SaleFactSynchronizer>();

        collection.AddScoped<ITagsService, TagsService>();
        return collection;
    }

    private static IServiceCollection RegisterMetricCalculators(this IServiceCollection collection)
    {
        collection.AddScoped<IMetricCalculatorFactory, MetricCalculatorFactory>();

        collection.AddScoped<IMetricCalculator<ProductSalesMetric>, ProductSalesMetricCalculator>();
        collection.AddScoped<IMetricCalculator<ProductPurchasesMetric>, ProductPurchasesMetricCalculator>();
        return collection;
    }
}
