using Application.Common;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Application.Common.Services;
using Application.Common.Services.Currency;
using Application.Common.Services.Settings;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Abstractions.Interfaces.Services;
using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Application.Services;
using Pricing.Application.Services.ArticlePricing;
using Pricing.Application.Services.ArticlePricing.BasePriceStrategies;
using SettingsService = Pricing.Application.Services.SettingsService;

namespace Pricing.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection)
    {
        collection.AddApplicationBase(typeof(Global).Assembly);

        collection.AddSingleton<ICurrencyConverter, CurrencyConverterBase>(_ =>
            new CurrencyConverterBase(Global.UsdId));

        collection.AddSingleton<IMarkupService, MarkupService>();
        collection.AddScoped<IMarkupSetup, MarkupSetup>();

        collection.AddSingleton<ICurrencyConverter, CurrencyConverterBase>(_ =>
            new CurrencyConverterBase(Global.UsdId));
        collection.AddScoped<ICurrencyConverterSetup, CurrencyConverterSetup>();

        collection.AddSingleton<ISettingsContainer, SettingsContainer>();
        collection.AddTransient<ISettingsService, SettingsService>();

        collection.AddSingleton<IBasePriceStrategyFactory, BasePriceStrategyFactory>();
        collection.AddSingleton<IBasePriceStrategy, AverageBasePriceStrategy>();
        collection.AddSingleton<IBasePriceStrategy, HighestBasePriceStrategy>();
        collection.AddSingleton<IBasePriceStrategy, LowestBasePriceStrategy>();
        collection.AddSingleton<IBasePriceStrategy, MedianBasePriceStrategy>();
        collection.AddSingleton<IBasePricesService, BasePriceService>();

        collection.AddSingleton<IDiscountService, DiscountService>();
        collection.AddSingleton<IPriceService, PriceService>();
        collection.AddScoped<ICurrencyService, CurrencyService>();

        return collection;
    }
}