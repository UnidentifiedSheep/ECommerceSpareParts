namespace Core.Interfaces.CacheRepositories;

public interface ICache
{
    Task StringSetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task StringSetAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> StringGetAsync(string key);
    Task<T?> StringGetAsync<T>(string key);
    Task DeleteAsync(string key);
    Task DeleteAsync(IEnumerable<string> keys);
    Task<IEnumerable<string?>> SetMembersAsync(string key);
    Task SetAddAsync(string key, string value);
    Task SetAddAsync(string key, IEnumerable<string> members);
    Task KeyExpireAsync(string key, TimeSpan? expiry = null);
}