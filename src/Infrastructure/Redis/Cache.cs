using Core.Interfaces.CacheRepositories;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace Redis;

public class Cache(IDatabase redis) : ICache
{
    private readonly JsonCommands _json = redis.JSON();
    public async Task StringSetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        await _json.SetAsync(key, "$", value, When.Always, Global.JsonOptions);
        await redis.KeyExpireAsync(key, expiry);
    }
    public async Task<T?> StringGetAsync<T>(string key, string path = "$")
    {
        var res = await _json.GetAsync<T>(key, path, Global.JsonOptions);
        return res;
    }

    public async Task<List<T?>> StringsGetAsync<T>(IEnumerable<string> keys, string path = "$")
    {
        var pipeline = new Pipeline(redis);
        var tasks = new List<Task<T?>>();

        foreach (var key in keys)
            tasks.Add(pipeline.Json.GetAsync<T>(key, path, Global.JsonOptions));

        pipeline.Execute();

        return (await Task.WhenAll(tasks)).ToList();
    }

    public void StringBatchSet<T>(IEnumerable<Tuple<string, T, TimeSpan?>> items)
    {
        var pipeline = new Pipeline(redis);

        foreach (var (key, value, expiry) in items)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            _ = pipeline.Json.SetAsync(key, "$", value);
            if (expiry.HasValue)
                _ = pipeline.Db.KeyExpireAsync(key, expiry);
        }

        pipeline.Execute();
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