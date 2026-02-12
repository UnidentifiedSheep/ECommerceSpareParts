using Abstractions.Interfaces.Currency;
using Analytics.Application.Services;
using Analytics.Core.Interfaces.Services;
using Application.Common;
using Application.Common.Behaviors;
using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection)
    {
        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        collection.AddScoped<ISellInfoService, SellInfoService>();
        collection.AddScoped<ICurrencyConverterSetup, CurrencyConverterSetup>();
        
        collection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(Global).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(RequestsDataLoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CacheBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(DbValidationBehavior<,>), ServiceLifetime.Scoped);
        });
        
        return collection;
    }
}