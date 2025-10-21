using Application.Common;
using Application.Common.Behaviors;
using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection)
    {
        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        
        collection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(Global).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(RequestsDataLoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CacheBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });
        
        return collection;
    }
}