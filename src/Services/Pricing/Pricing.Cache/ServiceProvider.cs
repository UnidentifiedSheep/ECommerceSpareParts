using Abstractions.Interfaces.Cache;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Cache.Repositories;

namespace Pricing.Cache;

public static class ServiceProvider
{
    public static IServiceCollection AddAppCacheLayer(this IServiceCollection collection)
    {
        collection.AddScoped<IArticlePricesCacheRepository, ArticlePricesCachesRepository>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            var ttl = TimeSpan.FromHours(48);
            return new ArticlePricesCachesRepository(cache, ttl);
        });
        
        collection.AddScoped<ICurrencyCacheRepository, CurrencyCacheRepository>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            var ttl = TimeSpan.FromHours(24);
            return new CurrencyCacheRepository(cache, ttl);
        });
        
        collection.AddScoped<IUserCacheRepository, UserCacheRepository>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            var ttl = TimeSpan.FromHours(24);
            return new UserCacheRepository(cache, ttl);
        });

        return collection;
    }
}