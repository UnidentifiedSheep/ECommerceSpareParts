using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Analytics.Abstractions.Interfaces.Application;
using Analytics.Application.Services;
using Analytics.Application.Services.Metrics;
using Analytics.Application.Services.Metrics.Calculators;
using Analytics.Entities.Metrics;
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
        collection.RegisterRelatedData()
            .RegisterMetricCalculators()
            .RegisterCachePolicies(typeof(ServiceProvider).Assembly);

        collection.AddSingleton<IJsonSerializer, JsonSerializer>();
        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        collection.AddScoped<ICurrencyConverterSetup, CurrencyConverterSetup>();

        collection.AddValidatorsFromAssembly(typeof(Global).Assembly);
        
        collection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(Global).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CacheBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(DbValidationBehavior<,>), ServiceLifetime.Scoped);
        });

        return collection;
    }

    private static IServiceCollection RegisterMetricCalculators(this IServiceCollection collection)
    {
        collection.AddSingleton<IMetricCalculatorRegistry, MetricCalculatorRegistry>();
        collection.AddScoped<IMetricCalculatorFactory, MetricCalculatorFactory>();
        collection.AddScoped<IMetricValidatorDispatcher, MetricValidatorDispatcher>();

        collection.AddScoped<IMetricCalculator<ArticleSalesMetric>, ArticleSalesMetricCalculator>();
        return collection;
    }
}