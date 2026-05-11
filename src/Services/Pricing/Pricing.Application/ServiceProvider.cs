using Application.Common;
using Application.Common.Interfaces.Settings;
using Application.Common.Services.Settings;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Abstractions.Interfaces.Services;
using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Application.Services.ArticlePricing;
using Pricing.Application.Services.ArticlePricing.BasePriceStrategies;

namespace Pricing.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection)
    {
        collection.AddApplicationBase(typeof(Global).Assembly);

        collection.AddSingleton<IMarkupService, MarkupService>();
        collection.AddScoped<IMarkupSetup, MarkupSetup>();

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

        return collection;
    }
}