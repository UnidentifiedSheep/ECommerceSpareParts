using System.Text.Json;
using Core.Interfaces.CacheRepositories;
using StackExchange.Redis;

namespace Redis.Repositories;

public class Cache(IDatabase redis) : ICache
{
    public async Task StringSetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var ser = JsonSerializer.Serialize(value, Global.JsonOptions);
        await redis.StringSetAsync(key, ser, expiry);
    }

    public async Task StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        await redis.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> StringGetAsync(string key)
    {
        return await redis.StringGetAsync(key);
    }

    public async Task<T?> StringGetAsync<T>(string key)
    {
        var cacheValue = await redis.StringGetAsync(key);
        if (!cacheValue.HasValue || cacheValue.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T?>(cacheValue.ToString(), Global.JsonOptions);
    }

    public async Task DeleteAsync(string key)
    {
        await redis.KeyDeleteAsync(key);
    }

    public async Task DeleteAsync(IEnumerable<string> keys)
    {
        var redisKeys = keys.Select(k => new RedisKey(k)).ToArray();
        await redis.KeyDeleteAsync(redisKeys);
    }
}