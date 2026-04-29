using Abstractions.Interfaces.Cache;
using Application.Common.Interfaces;

namespace Application.Common.Abstractions;

public abstract class SingleEntityCacheInvalidatorBase<TEntity, TKey>(
    ICache cache, 
    ICacheKeyRegistry keyRegistry) : ICacheInvalidator<TEntity, TKey>
{
    protected ICache Cache => cache;
    
    public async Task Invalidate(TKey key)
    {
        await Cache.DeleteAsync(keyRegistry.FormatKey<TEntity>(key));
    }

    public async Task Invalidate(IEnumerable<TKey> keys)
    {
        var cacheKeys = keys
            .Select(x => keyRegistry.FormatKey<TEntity>(x))
            .ToHashSet();
        await Cache.DeleteAsync(cacheKeys);
    }
}