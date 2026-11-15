using Main.Application.Products.Interfaces;
using Main.Modules.Product.Abstractions;

namespace Main.Application.Services;

public sealed class ProductCrosses(IProductProvider productProvider) : IProductCrosses
{
	public Task<IReadOnlyList<int>> GetCrossIdsAsync(int productId, CancellationToken cancellationToken = default)
		=> productProvider.GetProductCrossesAsync(productId, cancellationToken);
}
