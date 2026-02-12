using Abstractions.Interfaces.Cache;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Abstractions.Models.Pricing;

namespace Pricing.Cache.Repositories;

public class ArticlePricesCachesRepository : IArticlePricesCacheRepository
{
    private readonly ICache _cache;
    private readonly TimeSpan? _ttl;

    public ArticlePricesCachesRepository(ICache cache, TimeSpan? ttl = null)
    {
        _cache = cache;
        _ttl = ttl;
    }

    public async Task SetArticleBasePrice(BasePricingItemResult item, TimeSpan? ttl)
    {
        ttl ??= _ttl;
        var key = GetArticleBasePriceKey(item.Id);
        await _cache.StringSetAsync(key, item, ttl);
    }
    
    public void SetArticleBasePrices(IEnumerable<(BasePricingItemResult value, TimeSpan? exp)> items)
    {
        var data = items
            .Select(x => 
                new Tuple<string, BasePricingItemResult, TimeSpan?>
                    (
                        GetArticleBasePriceKey(x.value.Id), 
                        x.value, 
                        x.exp
                    )
            );
        _cache.StringBatchSet(data);
    }
    
    public async Task<BasePricingItemResult?> GetArticleBasePrice(int articleId)
    {
        var key = GetArticleBasePriceKey(articleId);
        var result = await _cache.StringGetAsync<BasePricingItemResult>(key);
        return result;
    }
    
    public async Task<Dictionary<int, BasePricingItemResult?>> GetArticleBasePrices(IEnumerable<int> articleIds)
    {
        var ids = articleIds.Distinct().ToList();
        var keys = ids.Select(GetArticleBasePriceKey);
        var result = await _cache.StringsGetAsync<BasePricingItemResult>(keys);
        var dict = new Dictionary<int, BasePricingItemResult?>();
        for (int i = 0; i < ids.Count; i++)
        {
            var id = ids[i];
            dict.Add(id, result[i]);
        }
        return dict;
    }

    public async Task<decimal?> GetFinalPrice(int articleId)
    {
        var key = GetArticleBasePriceKey(articleId);
        var result = await _cache.StringGetAsync<decimal>(key, "$.finalPrice");
        return result;
    }
    
    public async Task DeleteArticleBasePrice(int articleId)
    {
        var key = GetArticleBasePriceKey(articleId);
        await _cache.DeleteAsync(key);
    }

    public async Task DeleteArticleBasePrices(IEnumerable<int> articleIds)
    {
        var keys = articleIds.Select(GetArticleBasePriceKey).ToList();
        await _cache.DeleteAsync(keys);
    }

    private static string GetArticleBasePriceKey(int articleId)
    {
        return $"article-base-price:{articleId}";
    }
}