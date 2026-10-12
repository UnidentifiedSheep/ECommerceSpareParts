using Application.Common.Interfaces.Repositories;
using Main.Application.Models.Product;
using Main.Entities.Product;

namespace Main.Application.Interfaces.Persistence;

public interface IProductRepository : IRepository<Product, int>
{
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

	Task UpsertProductCrosses(
		IEnumerable<ProductCross> crosses,
		CancellationToken cancellationToken = default);

	Task InsertProductsOnConflictDoNothingAsync(
		IReadOnlyCollection<Product> products,
		CancellationToken cancellationToken = default);

	Task InsertProductEansOnConflictDoNothingAsync(
		IReadOnlyCollection<ProductEan> eans,
		CancellationToken cancellationToken = default);

	Task UpdateProductGroupsAsync(
		IReadOnlyCollection<ProductGroupAssignment> assignments,
		CancellationToken cancellationToken = default);
}
