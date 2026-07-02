using Main.Application.Dtos.Product;

namespace Main.Application.Interfaces.Cache;

public interface IProductCacheRepository
{
    Task<ProductDto> GetProductOrSetAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<ProductDto?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto?>> GetProductsAsync(
        IEnumerable<int> ids);

    Task<IEnumerable<int>> GetProductCrossesAsync(
        int productId,
        string? sortBy,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, ProductDto>> GetProductsOrSetAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);

    Task InvalidateProductAsync(int productId);

    Task InvalidateProductsAsync(IEnumerable<int> productIds);

    Task InvalidateCrossesAsync(int productId);
    Task InvalidateCrossesAsync(IEnumerable<int> productIds);
}