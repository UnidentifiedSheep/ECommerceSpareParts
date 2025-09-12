using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.CacheRepositories;
using Microsoft.Extensions.DependencyInjection;
using Redis.Factories;
using Redis.Repositories;

namespace Redis;

public static class ServiceProvider
{
    public static IServiceCollection AddCacheLayer(this IServiceCollection collection, string redisConnectionString)
    {
        Redis.Configure(redisConnectionString);
        
        collection.AddTransient<ICache, Cache>(_ =>
        {
            var redis = Redis.GetRedis();
            return new Cache(redis);
        });
        collection.AddTransient<IRelatedDataRepository<Article>, ArticleRelatedDataRepository>(_ =>
        {
            var redis = Redis.GetRedis();
            var ttl = TimeSpan.FromHours(10);
            return new ArticleRelatedDataRepository(redis, ttl);
        });
        collection.AddTransient<IRedisArticlePriceRepository, RedisArticlePricesRepository>(_ =>
        {
            var redis = Redis.GetRedis();
            var ttl = TimeSpan.FromHours(8);
            return new RedisArticlePricesRepository(redis, ttl);
        });
        collection.AddTransient<IRedisUserRepository, RedisUserRepository>(_ =>
        {
            var redis = Redis.GetRedis();
            var ttl = TimeSpan.FromHours(1);
            return new RedisUserRepository(redis, ttl);
        });

        collection.AddTransient<IRelatedDataRepositoryFactory, RelatedDataRepositoryFactory>();
        
        return collection;
    }
}