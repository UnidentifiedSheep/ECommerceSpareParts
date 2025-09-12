namespace Core.Interfaces.Services;

public interface IArticlePricesService
{
    Task<Dictionary<int, double?>> GetUsablePricesAsync(IEnumerable<int> articleIds, 
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, double>> RecalculateUsablePrice(IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default);
}