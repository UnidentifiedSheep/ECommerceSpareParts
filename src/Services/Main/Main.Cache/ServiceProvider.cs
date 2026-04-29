using Abstractions.Interfaces.Cache;
using Application.Common.Interfaces;
using Main.Application.Interfaces.CacheRepositories;
using Main.Cache.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Cache;

public static class ServiceProvider
{
    public static IServiceCollection AddAppCacheLayer(this IServiceCollection collection)
    {
        collection.AddScoped<IUsersCacheRepository, UsersCacheRepository>(sp => new UsersCacheRepository(
            redis: sp.GetRequiredService<ICache>(),
            keyRegistry: sp.GetRequiredService<ICacheKeyRegistry>(), 
            ttl: TimeSpan.FromHours(8)));

        return collection;
    }
}