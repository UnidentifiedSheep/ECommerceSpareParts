using Microsoft.Extensions.DependencyInjection;

namespace Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheLayer(
        this IServiceCollection serviceCollection, 
        string connectionString)
    {
        return serviceCollection.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
    }
}