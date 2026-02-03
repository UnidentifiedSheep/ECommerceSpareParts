using Core.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Cache.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Cache;

public static class ServiceProvider
{
    public static IServiceCollection AddAppCacheLayer(this IServiceCollection collection)
    {
        collection.AddScoped<IArticlePricesCacheRepository, ArticlePricesCachesRepository>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            var ttl = TimeSpan.FromHours(8);
            return new ArticlePricesCachesRepository(cache, ttl);
        });
        collection.AddScoped<IUsersCacheRepository, UsersCacheRepository>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            var ttl = TimeSpan.FromHours(8);
            return new UsersCacheRepository(cache, ttl);
        });

        return collection;
    }
}