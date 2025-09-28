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
    Task SetAddAsync(IEnumerable<string> keys, string member, TimeSpan? expiry = null);
    /// <summary>
    /// Adds to each key from keys, all members
    /// </summary>
    /// <param name="keys">redis keys</param>
    /// <param name="members">values</param>
    /// <param name="expiry">when expires</param>
    /// <returns>Task</returns>
    Task SetAddAsync(IEnumerable<string> keys, IEnumerable<string> members, TimeSpan? expiry = null);
    Task KeyExpireAsync(string key, TimeSpan? expiry = null);
}