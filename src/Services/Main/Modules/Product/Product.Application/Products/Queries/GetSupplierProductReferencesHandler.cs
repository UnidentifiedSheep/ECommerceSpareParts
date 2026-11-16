using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Modules.Product.Application.Dtos.Product.SupplierReferences;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Products.Queries;

public record GetSupplierProductReferencesQuery(IReadOnlyCollection<int> ProductIds, Supplier Supplier)
	: IQuery<GetSupplierProductReferencesResult>;

public record GetSupplierProductReferencesResult(IReadOnlyList<ResolvedSupplierProductReferenceDto> Products);

public class GetSupplierProductReferencesHandler(IReadRepository<Entities.Product.Product.Product, int> repository)
	: IQueryHandler<GetSupplierProductReferencesQuery, GetSupplierProductReferencesResult>
{
	public async Task<GetSupplierProductReferencesResult> Handle(
		GetSupplierProductReferencesQuery request,
		CancellationToken cancellationToken)
	{
		var ids = request.ProductIds.Distinct().ToList();
		if (ids.Count == 0)
			return new GetSupplierProductReferencesResult([]);

		var result = await repository
			.Query
			.Where(p => ids.Contains(p.Id))
			.SelectMany(
				p => p.Producer.SupplierMappings.Where(m => m.Supplier == request.Supplier).Take(1),
				(p, m) => new ResolvedSupplierProductReferenceDto
				{
					ProductId = p.Id,
					Sku = p.Sku.NormalizedValue,
					SupplierProducerName = m.Name
				})
			.ToListAsync(cancellationToken);

		return new GetSupplierProductReferencesResult(result);
	}
}
