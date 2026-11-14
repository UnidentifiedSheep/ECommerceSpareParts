using Enums;

namespace Main.Modules.Product.Abstractions;

public interface IProductReferenceResolver
{
	Task<IReadOnlyList<ResolvedProductReference>> ResolveAsync(
		IReadOnlyCollection<ProductReference> references,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<SupplierProductReference>> GetSupplierReferencesAsync(
		IReadOnlyCollection<int> productIds,
		Supplier supplier,
		CancellationToken cancellationToken = default);
}

public sealed record ProductReference(string Sku, string ProducerName, Supplier? Supplier);

public sealed record ResolvedProductReference(
	int? ProducerId,
	int? ProductId,
	string Sku,
	string ProducerName,
	Supplier? Supplier);

public sealed record SupplierProductReference(int ProductId, string Sku, string SupplierProducerName);
