using Core.Interfaces.CacheRepositories;

namespace Core.Abstractions;

public abstract class RelatedDataBase(ICache cache, TimeSpan? ttl = null)
{
    public virtual async Task<IEnumerable<string>> GetRelatedDataKeys(string id)
    {
        var key = GetRelatedDataKey(id);
        var members = await cache.SetMembersAsync(key);
        var ids = members
            .Where(x => x != null)
            .Select(x => x!.ToString());
        return ids;
    }

    public virtual async Task AddRelatedDataAsync(string id, string relatedKey)
    {
        var key = GetRelatedDataKey(id);
        await cache.SetAddAsync(key, relatedKey);
        await cache.KeyExpireAsync(key, ttl);
    }

    public virtual async Task AddRelatedDataAsync(IEnumerable<string> ids, string relatedKey)
    {
        var keys = ids.Select(GetRelatedDataKey);
        await cache.SetAddAsync(keys, relatedKey, ttl);
    }

    public virtual async Task AddRelatedDataAsync(string id, IEnumerable<string> relatedKeys)
    {
        var key = GetRelatedDataKey(id);
        await cache.SetAddAsync(key, relatedKeys);
        await cache.KeyExpireAsync(key, ttl);
    }
    public abstract string GetRelatedDataKey(string id);
}

public abstract class RelatedDataBase<TEntity>(ICache cache, TimeSpan? ttl = null) : RelatedDataBase(cache, ttl);