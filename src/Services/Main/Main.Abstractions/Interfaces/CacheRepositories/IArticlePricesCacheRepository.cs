using Main.Abstractions.Models.Pricing;

namespace Main.Abstractions.Interfaces.CacheRepositories;

public interface IArticlePricesCacheRepository
{
    Task SetArticleBasePrice(BasePricingItemResult item, TimeSpan? ttl);
    Task<BasePricingItemResult?> GetArticleBasePrice(int articleId);
    void SetArticleBasePrices(IEnumerable<(BasePricingItemResult value, TimeSpan? exp)> items);
    Task<Dictionary<int, BasePricingItemResult?>> GetArticleBasePrices(IEnumerable<int> articleIds);
    Task<decimal?> GetFinalPrice(int articleId);
    Task DeleteArticleBasePrice(int articleId);
    Task DeleteArticleBasePrices(IEnumerable<int> articleIds);
}