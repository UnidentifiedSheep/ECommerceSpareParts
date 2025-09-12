namespace Core.Interfaces.CacheRepositories;

public interface IRedisArticlePriceRepository
{
    Task<Dictionary<int, double?>> GetUsablePricesAsync(IEnumerable<int> articleIds);
    Task SetUsablePricesAsync(Dictionary<int, double> prices);
    Task SetUsablePriceAsync(int articleId, double price);
    Task<DateTime?> GetPriceUpdateTimeAsync(int articleId);
    Task<Dictionary<int, DateTime?>> GetPriceUpdateTimeAsync(IEnumerable<int> articleIds);
    Task SetPriceUpdateTimeAsync(int articleId, DateTime updateTime);
    Task SetPriceUpdateTimeAsync(IEnumerable<int> ids, DateTime updateTime);
}