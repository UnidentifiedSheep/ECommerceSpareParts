using Analytics.Application.Interfaces.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationCache(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyCacheRepository, CurrencyCacheRepository>();

        return services;
    }
}