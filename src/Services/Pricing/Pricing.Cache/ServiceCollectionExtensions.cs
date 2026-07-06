using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces.Cache;

namespace Pricing.Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationCache(this IServiceCollection services)
    {
        services.AddScoped<ICachedCurrencyProvider, CachedCurrencyProvider>();

        return services;
    }
}