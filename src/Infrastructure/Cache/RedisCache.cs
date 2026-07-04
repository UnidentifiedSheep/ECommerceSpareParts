using NRedisStack.Json.DataTypes;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace Cache;

public class RedisCache(
    IDatabase redis,
    string? prefix = null
) : ICache
{
    public Task<T?> GetAsync<T>(
        string key,
        string path = "$")
    {
        return redis.JSON()
            .GetAsync<T>(GetWithPrefix(key), path);
    }

    public Task<RedisResult[]> GetAsync<T>(
        IEnumerable<string> keys,
        string path = "$")
    {
        return redis.JSON()
            .MGetAsync(
                GetWithPrefixes(keys),
                path);
    }

    public async Task SetAsync<T>(
        IEnumerable<(string key, T value)> keyValues,
        TimeSpan? ttl = null)
    {
        var rawKeys = new HashSet<string>();
        var values = keyValues
            .Select(x =>
            {
                ArgumentNullException.ThrowIfNull(x.value);
                rawKeys.Add(x.key);
                return new KeyPathValue(
                    GetWithPrefixString(x.key),
                    "$",
                    x.value);
            })
            .ToArray();

        await redis.JSON().MSetAsync(values);
        await SetExpireAsync(rawKeys, ttl);
    }

    public Task<bool> SetExpireAsync(
        string key,
        TimeSpan? ttl = null,
        ExpireWhen when = ExpireWhen.Always)
    {
        return SetExpireCore(
            redis,
            key,
            ttl,
            when);
    }

    public async Task<Dictionary<string, bool>> SetExpireAsync(
        IEnumerable<string> keys,
        TimeSpan? ttl = null,
        ExpireWhen when = ExpireWhen.Always)
    {
        var batch = redis.CreateBatch();
        var keyedTasks = keys
            .Distinct()
            .Select(x => (x, SetExpireCore(
                batch,
                x,
                ttl,
                when)))
            .ToDictionary(x => x.x, x => x.Item2);

        batch.Execute();
        await Task.WhenAll(keyedTasks.Values);

        return keyedTasks.ToDictionary(
            x => x.Key,
            x => x.Value.Result);
    }

    public async Task<IEnumerable<T?>> GetEnumerableAsync<T>(
        string key,
        string path = "$[*]")
    {
        if (!await redis.KeyExistsAsync(GetWithPrefix(key))) return [];

        return await redis.JSON().GetEnumerableAsync<T>(GetWithPrefix(key), path);
    }

    public Task<bool> KeyExistsAsync(string key) { return redis.KeyExistsAsync(GetWithPrefix(key)); }

    public async Task SetEnumerableAsync<T>(
        string key,
        IEnumerable<T> values,
        string path = "$",
        TimeSpan? ttl = null)
    {
        await redis.JSON()
            .SetAsync(
                GetWithPrefix(key),
                path,
                values);

        await SetExpireAsync(key, ttl);
    }

    public Task AddToSetAsync(
        string key,
        IEnumerable<string> values,
        TimeSpan? ttl = null)
    {
        return AddToSetAsync(
            new Dictionary<string, List<string>>
            {
                [key] = values.ToList()
            },
            ttl);
    }

    public Task RemoveKeysAsync(IEnumerable<string> keys)
    {
        return redis.KeyDeleteAsync(GetWithPrefixes(keys));
    }

    public Task RemoveKeyAsync(string key) { return redis.KeyDeleteAsync(GetWithPrefix(key)); }

    public async Task<string[]> GetFromSetAsync(string key)
    {
        var result = await redis.SetMembersAsync(GetWithPrefix(key));
        return result
            .Where(x => x.HasValue)
            .Select(x => x.ToString())
            .ToArray();
    }

    public async Task<Dictionary<string, string[]>> GetFromManySetsAsync(IEnumerable<string> keys)
    {
        var distinctKeys = keys.Distinct().ToArray();
        var batch = redis.CreateBatch();
        var tasks = distinctKeys
            .Select(key => batch.SetMembersAsync(GetWithPrefix(key)))
            .ToList();

        batch.Execute();
        await Task.WhenAll(tasks);

        return distinctKeys.Zip(tasks.Select(x => x.Result))
            .ToDictionary(
                x => x.First,
                x => x.Second
                    .Where(z => z.HasValue)
                    .Select(y => y.ToString())
                    .ToArray());
    }

    public async Task AddToSetAsync(
        Dictionary<string, List<string>> keyValues,
        TimeSpan? ttl = null)
    {
        if (keyValues.Count == 0) return;

        var batch = redis.CreateBatch();
        var tasks = new List<Task>();
        foreach (var (key, values) in keyValues)
            tasks.Add(
                AddToSetCore(
                    batch,
                    key,
                    values));

        batch.Execute();
        await Task.WhenAll(tasks);

        if (ttl.HasValue) await SetExpireAsync(keyValues.Keys, ttl);
    }

    private Task AddToSetCore(
        IDatabaseAsync db,
        string key,
        IEnumerable<string> values)
    {
        return db.SetAddAsync(
            GetWithPrefix(key),
            values.Select(x => new RedisValue(x)).ToArray());
    }

    private Task<bool> SetExpireCore(
        IDatabaseAsync db,
        string key,
        TimeSpan? ttl,
        ExpireWhen when)
    {
        return db.KeyExpireAsync(
            GetWithPrefix(key),
            ttl,
            when);
    }

    private string GetWithPrefixString(string key)
    {
        return string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}:{key}";
    }

    private RedisKey GetWithPrefix(string key) { return GetWithPrefixString(key); }

    private RedisKey[] GetWithPrefixes(IEnumerable<string> keys)
    {
        return keys.Select(GetWithPrefix).ToArray();
    }
}