using MonoliteUnicorn.Models;

namespace MonoliteUnicorn.Services.Prices.Price;

public interface IPrice
{
    Task<double> GetPrice(int articleId, int currencyId, string? buyerId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, double>> GetPrices(IEnumerable<int> articleIds, int currencyId, string? buyerId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, DetailedPriceModel>> GetDetailedPrices(IEnumerable<int> articleIds, int currencyId, string? buyerId, CancellationToken cancellationToken = default);

    Task RecalculateUsablePrice(IEnumerable<(int ArticleId, decimal BuyPriceInUsd)> values,
        CancellationToken cancellationToken = default);
}