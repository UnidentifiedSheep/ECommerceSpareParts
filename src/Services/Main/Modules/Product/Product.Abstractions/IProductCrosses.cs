namespace Main.Modules.Product.Abstractions;

public interface IProductCrosses
{
	Task<IReadOnlyList<int>> GetCrossIdsAsync(int productId, CancellationToken cancellationToken = default);

	Task<IReadOnlyDictionary<int, IReadOnlyList<int>>> GetCrossIdsAsync(
		IReadOnlyCollection<int> productIds,
		CancellationToken cancellationToken = default);
}
