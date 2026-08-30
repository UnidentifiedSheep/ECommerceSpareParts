using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Entities.Product.Enrichment;

namespace Main.Application.Interfaces.Persistence;

public interface ISupplierProductRepository : IRepository<SupplierProduct, int>
{
	Task<IReadOnlyList<SupplierProduct>> GetBySupplierKeysAsync(
		Supplier supplier,
		IEnumerable<(string NormalizedSku, string Producer)> keys,
		CancellationToken cancellationToken = default);

	Task UpsertCrossesAsync(
		IEnumerable<SupplierProductCross> crosses,
		CancellationToken cancellationToken = default);
}
