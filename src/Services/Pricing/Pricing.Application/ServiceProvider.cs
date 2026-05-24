using Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces.Services;
using Pricing.Application.Interfaces.Services.Pricing;
using Pricing.Application.Services.ProductPricing;
using Pricing.Application.Services.ProductPricing.BasePriceStrategies;

namespace Pricing.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        collection.AddApplicationBase(configuration, typeof(Global).Assembly);

        collection.AddSingleton<IBasePriceStrategyFactory, BasePriceStrategyFactory>();
        collection.AddSingleton<IBasePriceStrategy, AverageBasePriceStrategy>();
        collection.AddSingleton<IBasePriceStrategy, HighestBasePriceStrategy>();
        collection.AddSingleton<IBasePriceStrategy, LowestBasePriceStrategy>();
        collection.AddSingleton<IBasePriceStrategy, MedianBasePriceStrategy>();
        collection.AddSingleton<IBasePricesService, BasePriceService>();

        collection.AddSingleton<IDiscountService, DiscountService>();
        collection.AddSingleton<IPriceService, PriceService>();
        collection.AddSingleton<IMarkupService, MarkupService>();

        return collection;
    }
}