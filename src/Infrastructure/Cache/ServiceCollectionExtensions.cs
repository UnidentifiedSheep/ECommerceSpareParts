using Microsoft.Extensions.DependencyInjection;

namespace Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheLayer(
        this IServiceCollection serviceCollection,
        string connectionString,
        string serviceName)
    {
        return serviceCollection.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = $"{serviceName}:";
        });
    }
}