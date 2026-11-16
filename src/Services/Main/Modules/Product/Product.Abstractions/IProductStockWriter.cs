namespace Main.Modules.Product.Abstractions;

public sealed record ProductStockDelta(string StorageCode, int ProductId, int Delta);

public interface IProductStockWriter
{
	Task ApplyAsync(
		IReadOnlyCollection<ProductStockDelta> deltas,
		CancellationToken cancellationToken = default);
}
