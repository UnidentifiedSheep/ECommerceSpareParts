using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Application.Common.Services.Currency;
using Application.Common.Services.Settings;
using Application.Common.Validators;
using Main.Application.Configs;
using Main.Application.Handlers.Users.GetUserDiscount;
using Main.Application.Interfaces.Logistics;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.Currency;
using Main.Application.Interfaces.Services.Storage;
using Main.Application.Lrts.ProducerImport;
using Main.Application.Services;
using Main.Application.Services.Currency;
using Main.Application.Services.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Application.Services.Storage;
using Main.Entities.Balance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        IConfiguration? configuration)
    {
        collection
            .AddNamedObjects()
            .AddLrtLayer(typeof(ProducerImportLrt).Assembly)
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer();

        collection.AddSingleton<IJwtGenerator, JwtGenerator>();

        collection.AddScoped<ICurrencyConverter, CurrencyConverter>();

        collection.RegisterSettingsService<SettingFactory>();

        collection.AddSingleton<ILogisticsPricingStrategy, NonePricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaAndWeight>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaOrWeightPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerOrderPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerWeightPricing>();
        collection.AddSingleton<ILogisticsCostService, LogisticsCostService>();

        collection.AddScoped<IStorageContentChangeNotifier, StorageContentChangeNotifier>();

        collection.AddScoped<IMailingService, MailingService>();
        collection.AddScoped<ITransactionFinancialProfileService, TransactionFinancialProfileService>();
        collection.AddScoped<IBalanceService, BalanceService>();
        collection.AddScoped<IPurchaseLogisticsService, PurchaseLogisticsService>();
        collection.AddScoped<ISaleService, SaleService>();
        collection.AddScoped<IUserTokenService, UserTokenService>();
        collection.AddScoped<ICurrencyRateUpdater, CurrencyRateUpdater>();
        collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();

        collection.AddApplicationBase(configuration, typeof(Global).Assembly);

        collection.AddSingleton<IEmailValidator, EmailValidator>();

        collection.Scan(scan => scan
            .FromAssemblyOf<GetUserDiscountHandler>()
            .AddClasses(classes => classes.Where(type =>
                type.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(ILoggableRequest<>))))
            .As(type => type.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(ILoggableRequest<>)))
            .WithScopedLifetime()
        );

        ValidationConfiguration.Configure();

        return collection;
    }
}
