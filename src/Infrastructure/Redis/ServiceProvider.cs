using Abstractions.Interfaces.Cache;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Redis;

public static class ServiceProvider
{
    public static IServiceCollection AddCacheLayer(this IServiceCollection collection, string redisConnectionString, 
        string? prefix = null)
    {
        collection.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        
        collection.AddScoped<IDatabase>(sp =>
        {
            var mux = sp.GetRequiredService<IConnectionMultiplexer>();
            return mux.GetDatabase();
        });


        collection.AddScoped<ICache, Cache>(sp =>
        {
            var redis = sp.GetRequiredService<IDatabase>();
            return new Cache(redis, prefix);
        });

        return collection;
    }
}