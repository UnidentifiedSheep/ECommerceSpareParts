using Abstractions.Interfaces.Cache;
using Application.Common.Interfaces;

namespace Application.Common.Abstractions;

public abstract class SingleEntityCacheInvalidatorBase<TEntity, TKey>(
    ICache cache, 
    ICacheKey<TEntity> cacheKey) : ICacheInvalidator<TEntity, TKey>
{
    protected ICache Cache => cache;
    protected ICacheKey<TEntity> CacheKey => cacheKey;
    
    public Task Invalidate(TKey key)
    {
        Cache.DeleteAsync(CacheKey.FormatKey(key));
        return Task.CompletedTask;
    }

    public Task Invalidate(IEnumerable<TKey> keys)
    {
        var cacheKeys = keys
            .Select(x => CacheKey.FormatKey(x))
            .ToHashSet();
        Cache.DeleteAsync(cacheKeys);
        return Task.CompletedTask;
    }
}