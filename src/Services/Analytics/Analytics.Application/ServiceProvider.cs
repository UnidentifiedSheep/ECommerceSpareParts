using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Analytics.Abstractions.Interfaces.Application;
using Analytics.Application.Configs.Mapster;
using Analytics.Application.Services;
using Analytics.Application.Services.Metrics;
using Analytics.Application.Services.Metrics.Calculators;
using Analytics.Application.Services.Metrics.Converters;
using Analytics.Application.Services.Metrics.Validators;
using Analytics.Entities.Metrics;
using Application.Common;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Application.Common.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection)
    {
        MapsterConfig.Configure();
        
        collection
            .AddApplicationBase(typeof(Global).Assembly)
            .RegisterMetricCalculators()
            .RegisterMetricConverters();

        collection.AddSingleton<IJsonSerializer, JsonSerializer>();
        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        collection.AddScoped<ICurrencyConverterSetup, CurrencyConverterSetup>();

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