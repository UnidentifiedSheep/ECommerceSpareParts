using StackExchange.Redis;

namespace Cache;

public interface ICache
{
    Task<T?> GetAsync<T>(
        string key,
        string path = "$");

    Task<RedisResult[]> GetAsync<T>(
        IEnumerable<string> keys,
        string path = "$");

    Task SetAsync<T>(
        IEnumerable<(string key, T value)> keyValues,
        TimeSpan? ttl = null);

    Task<bool> SetExpireAsync(
        string key,
        TimeSpan? ttl = null,
        ExpireWhen when = ExpireWhen.Always);

    Task<Dictionary<string, bool>> SetExpireAsync(
        IEnumerable<string> keys,
        TimeSpan? ttl = null,
        ExpireWhen when = ExpireWhen.Always);

    Task<IEnumerable<T?>> GetEnumerableAsync<T>(
        string key,
        string path = "$[*]");

    Task<bool> KeyExistsAsync(string key);

    Task SetEnumerableAsync<T>(
        string key,
        IEnumerable<T> values,
        string path = "$",
        TimeSpan? ttl = null);

    Task AddToSetAsync(
        string key,
        IEnumerable<string> values,
        TimeSpan? ttl = null);

    Task RemoveKeysAsync(IEnumerable<string> keys);
    Task RemoveKeyAsync(string key);

    Task<string[]> GetFromSetAsync(string key);

    Task AddToSetAsync(
        Dictionary<string, List<string>> keyValues,
        TimeSpan? ttl = null);
}