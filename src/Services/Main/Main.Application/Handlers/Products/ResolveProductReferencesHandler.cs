using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product.References;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Product.ValueObjects;

namespace Main.Application.Handlers.Products;

public record ResolveProductReferencesQuery(IReadOnlyCollection<ProductReferenceDto> References)
	: IQuery<ResolveProductReferencesResult>;

public record ResolveProductReferencesResult(IReadOnlyList<ResolvedProductReferenceDto> Products);

public sealed class ResolveProductReferencesHandler(IProductRepository repository, IProducerLookupService lookupService)
	: IQueryHandler<ResolveProductReferencesQuery, ResolveProductReferencesResult>
{
	public async Task<ResolveProductReferencesResult> Handle(
		ResolveProductReferencesQuery request,
		CancellationToken cancellationToken)
	{
		if (request.References.Count == 0)
			return new ResolveProductReferencesResult([]);

		var lookup = await lookupService.Load(cancellationToken);
		var references = request.References.Select(reference => new
		{
			Reference = reference,
			ProducerId = lookup.ResolveId(reference.ProducerName, reference.Supplier),
			NormalizedSku = string.IsNullOrWhiteSpace(reference.Sku) ? string.Empty : Sku.ToNormalized(reference.Sku)
		}).ToArray();
		var keys = references
			.Where(reference => reference.ProducerId.HasValue && reference.NormalizedSku.Length != 0)
			.Select(reference => (reference.NormalizedSku, reference.ProducerId!.Value));
		var productIds = await repository.GetProductIdsByKeys(keys, cancellationToken);

		return new ResolveProductReferencesResult(references.Select(reference => new ResolvedProductReferenceDto
		{
			Sku = reference.Reference.Sku,
			ProducerName = reference.Reference.ProducerName,
			Supplier = reference.Reference.Supplier,
			ProductId = reference.ProducerId is { } producerId &&
				productIds.TryGetValue((reference.NormalizedSku, producerId), out var productId) ? productId : null
		}).ToArray());
	}
}
