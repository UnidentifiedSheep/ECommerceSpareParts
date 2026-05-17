using Abstractions.Interfaces;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Services;
using Analytics.Application.Services.Metrics.Calculators;
using Analytics.Application.Services.Metrics.Converters;
using Analytics.Application.Services.Metrics.Validators;
using Analytics.Entities.Metrics;
using Application.Common;
using Application.Common.Interfaces.Currency;
using Application.Common.Services;
using Application.Common.Services.Currency;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection)
    {
        collection
            .AddApplicationBase(typeof(Global).Assembly)
            .RegisterMetricCalculators()
            .RegisterMetricConverters();

        collection.AddSingleton<IJsonSerializer, JsonSerializer>();
        collection.AddScoped<ICurrencyConverter, CurrencyConverter>();
        collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();

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