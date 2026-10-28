using Application.Common.Interfaces.Repositories;
using Main.Application.Models.Product;
using Main.Entities.Product;
using Main.Enums.Products;

namespace Main.Application.Interfaces.Persistence;

public interface IProductRepository : IRepository<Product, int>
{
	Task<ProductDeletionBlocker?> GetDeletionBlockerAsync(int productId, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<Product>> GetProductCrosses(
		int productId,
		Criteria<Product> criteria,
		CancellationToken cancellationToken = default);

	Task<HashSet<(string NormalizedSku, int ProducerId)>> GetExistingProductKeys(
		IEnumerable<(string NormalizedSku, int ProducerId)> keys,
		CancellationToken cancellationToken = default);

	Task<Dictionary<(string NormalizedSku, int ProducerId), int>> GetProductIdsByKeys(
		IEnumerable<(string NormalizedSku, int ProducerId)> keys,
		CancellationToken cancellationToken = default);

	Task InsertProductCrossesOnConflictDoNothingAsync(
		IEnumerable<ProductCross> crosses,
		CancellationToken cancellationToken = default);

	Task DeleteProductCrossesAsync(
		IEnumerable<ProductCross> crosses,
		CancellationToken cancellationToken = default);

	Task InsertProductsOnConflictDoNothingAsync(
		IReadOnlyCollection<Product> products,
		CancellationToken cancellationToken = default);

	Task InsertProductEansOnConflictDoNothingAsync(
		IReadOnlyCollection<ProductEan> eans,
		CancellationToken cancellationToken = default);

	Task InsertProductImagesOnConflictDoNothingAsync(
		IReadOnlyCollection<ProductImage> images,
		CancellationToken cancellationToken = default);

	Task UpsertProductCharacteristicsAsync(
		IReadOnlyCollection<ProductCharacteristic> characteristics,
		CancellationToken cancellationToken = default);

	Task UpsertProductContentsAsync(
		IReadOnlyCollection<ProductContent> contents,
		CancellationToken cancellationToken = default);

	Task UpdateProductGroupsAsync(
		IReadOnlyCollection<ProductGroupAssignment> assignments,
		CancellationToken cancellationToken = default);
}
