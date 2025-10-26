using Analytics.Application.EventHandlers;
using Analytics.Application.Services;
using Analytics.Core.Interfaces.Services;
using Application.Common;
using Application.Common.Behaviors;
using Contracts.Currency;
using Contracts.Sale;
using Core.Interfaces;
using Core.Interfaces.MessageBroker;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection)
    {
        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        collection.AddScoped<ISellInfoService, SellInfoService>();
        
        collection.AddScoped<IEventHandler<CurrencyCreatedEvent>, CurrencyCreatedEventHandler>();
        collection.AddScoped<IEventHandler<CurrencyRateChangedEvent>, CurrencyRatesChangedEventHandler>();
        collection.AddScoped<IEventHandler<SaleCreatedEvent>, SaleCreatedEventHandler>();
        collection.AddScoped<IEventHandler<SaleEditedEvent>, SaleEditedEventHandler>();
        collection.AddScoped<IEventHandler<SaleDeletedEvent>, SaleDeletedEventHandler>();
        
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