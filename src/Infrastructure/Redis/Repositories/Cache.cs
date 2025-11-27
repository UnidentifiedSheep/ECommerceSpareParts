using System.Text.Json;
using Core.Interfaces.CacheRepositories;
using StackExchange.Redis;

namespace Redis.Repositories;

public class Cache(IDatabase redis) : ICache
{
    public async Task StringSetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var ser = JsonSerializer.Serialize(value, Global.JsonOptions);
        await redis.StringSetAsync(key, ser);
        await redis.KeyExpireAsync(key, expiry);
    }

    public async Task StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        await redis.StringSetAsync(key, value);
        await redis.KeyExpireAsync(key, expiry);
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

    public async Task<IEnumerable<string?>> SetMembersAsync(string key)
    {
        return (await redis.SetMembersAsync(key))
            .Select(x => x.HasValue ? x.ToString() : null);
    }

    public async Task SetAddAsync(string key, string value)
    {
        await redis.SetAddAsync(key, value);
    }

    public async Task SetAddAsync(string key, IEnumerable<string> members)
    {
        var values = members.Select(k => new RedisValue(k)).ToArray();
        await redis.SetAddAsync(key, values);
    }

    public async Task KeyExpireAsync(string key, TimeSpan? expiry = null)
    {
        await redis.KeyExpireAsync(key, expiry);
    }

    public async Task SetAddAsync(IEnumerable<string> keys, string member, TimeSpan? expiry = null)
    {
        var batch = redis.CreateBatch();
        var tasks = new List<Task>();

        foreach (var key in keys)
        {
            tasks.Add(batch.SetAddAsync(key, member));
            tasks.Add(batch.KeyExpireAsync(key, expiry));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    public async Task SetAddAsync(IEnumerable<string> keys, IEnumerable<string> members, TimeSpan? expiry = null)
    {
        var mems = members.ToList();
        var batch = redis.CreateBatch();
        var tasks = new List<Task>();

        foreach (var key in keys)
        {
            var values = mems.Select(k => new RedisValue(k)).ToArray();
            tasks.Add(batch.SetAddAsync(key, values));
            tasks.Add(batch.KeyExpireAsync(key, expiry));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }
}