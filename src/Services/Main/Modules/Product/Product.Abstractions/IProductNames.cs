namespace Main.Modules.Product.Abstractions;

public interface IProductNames
{
	Task<IReadOnlyDictionary<int, ProductDisplayName>> GetAsync(
		IReadOnlyCollection<int> productIds,
		CancellationToken cancellationToken = default);
}

public sealed record ProductDisplayName(string Name, string Sku);
