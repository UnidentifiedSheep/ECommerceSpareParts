using Enums;
using Main.Application.Dtos.Product.References;
using Main.Application.Products.Queries;
using Main.Modules.Product.Abstractions;
using MediatR;

namespace Main.Application.Services;

public sealed class ProductReferenceResolver(ISender sender) : IProductReferenceResolver
{
	public async Task<IReadOnlyList<ResolvedProductReference>> ResolveAsync(
		IReadOnlyCollection<ProductReference> references,
		CancellationToken cancellationToken = default)
	{
		var result = await sender.Send(
			new ResolveProductReferencesQuery(
				references
					.Select(reference => new ProductReferenceDto
					{
						Sku = reference.Sku, ProducerName = reference.ProducerName, Supplier = reference.Supplier
					})
					.ToArray()),
			cancellationToken);

		return result.Products
			.Select(product => new ResolvedProductReference(
				product.ProducerId,
				product.ProductId,
				product.Sku,
				product.ProducerName,
				product.Supplier))
			.ToArray();
	}

	public async Task<IReadOnlyList<SupplierProductReference>> GetSupplierReferencesAsync(
		IReadOnlyCollection<int> productIds,
		Supplier supplier,
		CancellationToken cancellationToken = default)
	{
		var result = await sender.Send(
			new GetSupplierProductReferencesQuery(productIds, supplier),
			cancellationToken);

		return result.Products
			.Select(product => new SupplierProductReference(
				product.ProductId,
				product.Sku,
				product.SupplierProducerName))
			.ToArray();
	}
}
