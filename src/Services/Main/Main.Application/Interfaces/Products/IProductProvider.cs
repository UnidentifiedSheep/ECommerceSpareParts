using Main.Application.Dtos.Product;
using Main.Application.Models.Product;

namespace Main.Application.Interfaces.Products;

public interface IProductProvider
{
	Task<ProductDto> GetProductOrSetAsync(int productId, CancellationToken cancellationToken = default);

	Task<ProductDto?> GetProductAsync(int productId, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<ProductDto?>> GetProductsAsync(IEnumerable<int> ids);

	Task<IReadOnlyList<int>> GetProductCrossesAsync(
		int productId,
		string[]? sortBy,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyDictionary<ProductCrossesRequestItem, IReadOnlyList<int>>> GetProductsCrossesAsync(
		IEnumerable<ProductCrossesRequestItem> requests,
		CancellationToken cancellationToken = default);

	Task<Dictionary<int, ProductDto>> GetProductsOrSetAsync(
		IEnumerable<int> ids,
		CancellationToken cancellationToken = default);
}
