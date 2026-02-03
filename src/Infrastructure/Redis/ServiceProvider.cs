using Core.Interfaces.CacheRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Redis;

public static class ServiceProvider
{
    public static IServiceCollection AddCacheLayer(this IServiceCollection collection, string redisConnectionString)
    {
        Redis.Configure(redisConnectionString);

        collection.AddScoped<ICache, Cache>(_ =>
        {
            var redis = Redis.GetRedis();
            return new Cache(redis);
        });

        return collection;
    }
}