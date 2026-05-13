using Main.Application.Interfaces.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationCache(this IServiceCollection services)
    {
        services.AddScoped<IProductCacheRepository, ProductCacheRepository>();
        return services;
    }
}