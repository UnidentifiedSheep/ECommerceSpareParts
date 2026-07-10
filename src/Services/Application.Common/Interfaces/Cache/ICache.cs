namespace Application.Common.Interfaces.Cache;

public interface ICache
{
    Task<T?> GetAsync<T>(string key);

    Task<IReadOnlyList<T?>> GetAsync<T>(IEnumerable<string> keys);

    Task SetAsync<T>(
        IEnumerable<(string key, T value)> keyValues,
        TimeSpan? ttl = null);
    
    Task SetAsync<T>(
        string key, 
        T value,
        TimeSpan? ttl = null);

    Task<bool> SetExpireAsync(
        string key,
        TimeSpan? ttl = null);

    Task<Dictionary<string, bool>> SetExpireAsync(
        IEnumerable<string> keys,
        TimeSpan? ttl = null);

    Task<IEnumerable<T?>> GetEnumerableAsync<T>(string key);

    Task<bool> KeyExistsAsync(string key);
    Task<Dictionary<string, bool>> KeyExistsAsync(
        IEnumerable<string> keys,
        CancellationToken token = default);

    Task SetEnumerableAsync<T>(
        string key,
        IEnumerable<T> values,
        TimeSpan? ttl = null);

    Task AddToSetAsync(
        string key,
        IEnumerable<string> values,
        TimeSpan? ttl = null);

    Task RemoveKeysAsync(IEnumerable<string> keys);
    Task RemoveKeyAsync(string key);

    Task<string[]> GetFromSetAsync(string key);
    Task<Dictionary<string, string[]>> GetFromManySetsAsync(IEnumerable<string> keys);

    Task AddToSetAsync(
        Dictionary<string, List<string>> keyValues,
        TimeSpan? ttl = null);
}