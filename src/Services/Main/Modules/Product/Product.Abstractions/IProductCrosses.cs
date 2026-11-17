namespace Main.Modules.Product.Abstractions;

public interface IProductCrosses
{
	Task<IReadOnlyList<int>> GetCrossIdsAsync(int productId, CancellationToken cancellationToken = default);
}
