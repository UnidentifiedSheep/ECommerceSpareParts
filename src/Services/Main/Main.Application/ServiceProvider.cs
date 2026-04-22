using System.Reflection;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.Currency;
using Abstractions.Interfaces.RelatedData;
using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Application.Common;
using Application.Common.Abstractions.Settings;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models;
using Main.Application.ConcurrencyValidator;
using Main.Application.Configs;
using Main.Application.Handlers.Users.GetUserDiscount;
using Main.Application.HangFireTasks;
using Main.Application.RelatedData;
using Main.Application.Services;
using Main.Application.Services.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Entities;
using Main.Entities.Producer;
using Main.Entities.Storage;
using Microsoft.Extensions.DependencyInjection;
using Currency = Main.Entities.Currency.Currency;
using User = Main.Entities.User.User;

namespace Main.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        UserEmailOptions? emailOptions = null,
        UserPhoneOptions? phoneOptions = null)
    {
        var relatedDataTtl = TimeSpan.FromHours(10);

        collection.AddSingleton<UpdateCurrencyRate>();
        collection.AddSingleton(emailOptions ?? new UserEmailOptions());
        collection.AddSingleton(phoneOptions ?? new UserPhoneOptions());

        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        collection.AddScoped<ICurrencyConverterSetup, CurrencyConverterSetup>();

        collection.AddSingleton<ISettingsContainer, SettingsContainer>();
        collection.AddScoped<ISettingsService, SettingsService>();

        collection.AddSingleton<ILogisticsPricingStrategy, NonePricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaAndWeight>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaOrWeightPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerAreaPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerOrderPricing>();
        collection.AddSingleton<ILogisticsPricingStrategy, PerWeightPricing>();
        collection.AddSingleton<ILogisticsCostService, LogisticsCostService>();

        collection.AddScoped<IStorageContentService, StorageContentService>();
        collection.AddScoped<IBalanceService, BalanceService>();
        collection.AddScoped<ISaleService, SaleService>();
        collection.AddScoped<IUserTokenService, UserTokenService>();
        collection.AddScoped<IPurchaseService, PurchaseService>();
        collection.AddScoped<IUserService, UserService>();

        collection.AddApplicationBase(typeof(Global).Assembly);

        collection.AddSingleton<IEmailValidator, EmailValidator>();
        collection.AddSingleton<IConcurrencyValidator<StorageContent>, StorageContentConcurrencyValidator>();

        collection.AddTransient<IRelatedDataRepository<ArticleCross>, ArticleCrossesRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new ArticleCrossesRelatedData(cache, relatedDataTtl);
        });
        collection.AddTransient<IRelatedDataRepository<Producer>, ProducerRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new ProducerRelatedData(cache, relatedDataTtl);
        });
        
        collection.AddTransient<IRelatedDataRepository<User>, UserRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new UserRelatedData(cache, relatedDataTtl);
        });

        collection.AddTransient<IRelatedDataRepository<Currency>, CurrencyRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new CurrencyRelatedData(cache, relatedDataTtl);
        });

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