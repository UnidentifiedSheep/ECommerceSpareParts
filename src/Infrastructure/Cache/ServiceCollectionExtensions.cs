using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheLayer(
        this IServiceCollection serviceCollection,
        string serviceName)
    {
        serviceCollection.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });

        serviceCollection.AddTransient<IDatabase>(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        serviceCollection.AddTransient<ICache, RedisCache>(sp => new RedisCache(
            sp.GetRequiredService<IDatabase>(),
            serviceName));

        serviceCollection.AddOptions<RedisCacheOptions>()
            .Configure<IOptions<RedisOptions>>((cacheOptions, redisOptions) =>
            {
                cacheOptions.Configuration = redisOptions.Value.ConnectionString;
                cacheOptions.InstanceName = $"{serviceName}:";
            });

        return serviceCollection.AddStackExchangeRedisCache(_ => { });
    }
}