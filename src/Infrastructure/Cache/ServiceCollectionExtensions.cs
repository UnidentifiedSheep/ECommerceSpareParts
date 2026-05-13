using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheLayer(
        this IServiceCollection serviceCollection,
        string connectionString,
        string serviceName)
    {
        serviceCollection.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
        serviceCollection.AddTransient<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        serviceCollection.AddTransient<ICache, RedisCache>(
            sp => new RedisCache(
                sp.GetRequiredService<IDatabase>(), 
                serviceName));
        
        return serviceCollection.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = $"{serviceName}:";
        });
    }
}