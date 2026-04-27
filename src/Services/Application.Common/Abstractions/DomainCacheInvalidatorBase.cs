using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Application.Common.Interfaces;

namespace Application.Common.Abstractions;

public abstract class DomainCacheInvalidatorBase<TEntity, TKey>(
    IRelatedDataRepository<TEntity> relatedDataRepository,
    ICache cache) : ICacheInvalidator<TEntity, TKey>
{
    protected IRelatedDataRepository<TEntity> RelatedDataRepository => relatedDataRepository;
    protected ICache Cache => cache;
    
    public virtual async Task Invalidate(TKey key)
    {
        var keyAsString = ValidateAndExtractKey(key);
        
        var keys = new HashSet<string>(await RelatedDataRepository.GetRelatedDataKeys(keyAsString))
        {
            RelatedDataRepository.GetRelatedDataKey(keyAsString)
        };

        await Cache.DeleteAsync(keys);
    }

    public async Task Invalidate(IEnumerable<TKey> keys)
    {
        var keysAsString = keys.Select(ValidateAndExtractKey).ToHashSet();
        var toDelete = new HashSet<string>(keysAsString.Select(RelatedDataRepository.GetRelatedDataKey));
        
        foreach (var key in keysAsString)
            toDelete.UnionWith(await RelatedDataRepository.GetRelatedDataKeys(key));
        
        await Cache.DeleteAsync(toDelete);
    }
    
    protected static string ValidateAndExtractKey(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        string? keyAsString = key.ToString();
        ArgumentNullException.ThrowIfNull(keyAsString);
        return keyAsString;
    }
}