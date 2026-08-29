namespace Main.Application.Interfaces.Cache;

public interface IProductCacheInvalidator
{
    Task InvalidateProductAsync(int productId);

    Task InvalidateProductsAsync(IEnumerable<int> productIds);

    Task InvalidateCrossesAsync(int productId);

    Task InvalidateCrossesAsync(IEnumerable<int> productIds);
}
