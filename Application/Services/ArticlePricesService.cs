using Core.Interfaces.CacheRepositories;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;

namespace Application.Services;

public class ArticlePricesService(
    IRedisArticlePriceRepository redisArticlePrice,
    IStorageContentRepository storageContentRepository) : IArticlePricesService
{
    public async Task<Dictionary<int, double?>> GetUsablePricesAsync(IEnumerable<int> articleIds,
        CancellationToken cancellationToken)
    {
        var foundPrices = await redisArticlePrice.GetUsablePricesAsync(articleIds);
        var notFound = foundPrices.Where(x => x.Value is null or <= 0)
            .Select(x => x.Key).ToHashSet();

        if (notFound.Count == 0) return foundPrices;

        var updated = await GetAlreadyUpdated(notFound);
        foreach (var id in updated)
            notFound.Remove(id);
        var newPrices = await RecalculateUsablePrice(notFound, cancellationToken);
        foreach (var (id, price) in newPrices)
            foundPrices[id] = price;
        return foundPrices;
    }

    public async Task<Dictionary<int, double>> RecalculateUsablePrice(IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        var ids = articleIds.Distinct().ToHashSet();

        var highestBuyPrices = await storageContentRepository
            .GetHighestBuyPrices(ids, 5, false, cancellationToken);
        var usablePrices = new Dictionary<int, double>();
        foreach (var (articleId, buyPrices) in highestBuyPrices)
        {
            var avg = Math.Round(buyPrices.Average(), 2);
            usablePrices[articleId] = (double)avg;
        }

        await redisArticlePrice.SetUsablePricesAsync(usablePrices);
        await redisArticlePrice.SetPriceUpdateTimeAsync(usablePrices.Keys, DateTime.Now);
        return usablePrices;
    }

    private async Task<IEnumerable<int>> GetAlreadyUpdated(IEnumerable<int> articleIds)
    {
        var res = await redisArticlePrice.GetPriceUpdateTimeAsync(articleIds);
        return res.Where(x => x.Value != null).Select(x => x.Key);
    }
}