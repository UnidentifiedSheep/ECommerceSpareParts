using Main.Modules.Product.Abstractions;
using Main.Modules.Product.Application.Products.Interfaces;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductCrosses(IProductProvider productProvider) : IProductCrosses
{
	public Task<IReadOnlyList<int>> GetCrossIdsAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> productProvider.GetProductCrossesAsync(productId, cancellationToken);
}
