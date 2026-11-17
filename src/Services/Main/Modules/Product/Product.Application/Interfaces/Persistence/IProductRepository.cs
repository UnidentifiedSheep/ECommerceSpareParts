using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Models.Product;

namespace Main.Modules.Product.Application.Interfaces.Persistence;

public interface IProductRepository : IRepository<Entities.Product.Product.Product, int>
{
	Task<IReadOnlyList<Entities.Product.Product.Product>> GetProductCrosses(
		int productId,
		Criteria<Entities.Product.Product.Product> criteria,
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

	Task<IReadOnlyList<int>> InsertProductsOnConflictDoNothingAsync(
		IReadOnlyCollection<Entities.Product.Product.Product> products,
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

	Task<IReadOnlyList<int>> UpdateProductGroupsAsync(
		IReadOnlyCollection<ProductGroupAssignment> assignments,
		CancellationToken cancellationToken = default);
}
