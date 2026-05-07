using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Application.Common;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Application.Common.Validators;
using Localization.Abstractions.Models;
using Localization.Domain.Extensions;
using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Configs;
using Main.Application.Handlers.Users.GetUserDiscount;
using Main.Application.HangFireTasks;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.Currency;
using Main.Application.Services;
using Main.Application.Services.Currency;
using Main.Application.Services.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        UserEmailOptions? emailOptions = null,
        UserPhoneOptions? phoneOptions = null)
    {
        Locale[] locales = ["ru-RU", "en-EN"];
        Locale defaultLocale = "ru-RU";

        collection
            .AddNamedObjects()
            .AddLocalization(defaultLocale, locales)
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer();
        
        collection.AddSingleton<IJwtGenerator, JwtGenerator>();
        collection.AddSingleton<UpdateCurrencyRate>();
        collection.AddSingleton(emailOptions ?? new UserEmailOptions());
        collection.AddSingleton(phoneOptions ?? new UserPhoneOptions());

        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>();

        collection.AddSingleton<ISettingFactory, SettingFactory>();

        collection.AddSingleton<ILogisticsPricingStrategy, NonePricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaAndWeight>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaOrWeightPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerOrderPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerWeightPricing>();
        collection.AddSingleton<ILogisticsCostService, LogisticsCostService>();

        collection.AddScoped<IBalanceService, BalanceService>();
        collection.AddScoped<ISaleService, SaleService>();
        collection.AddScoped<IUserTokenService, UserTokenService>();
        collection.AddScoped<IUserService, UserService>();
        collection.AddScoped<ICurrencyRateUpdater, CurrencyRateUpdater>();
        collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();

        collection.AddApplicationBase(typeof(Global).Assembly);

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