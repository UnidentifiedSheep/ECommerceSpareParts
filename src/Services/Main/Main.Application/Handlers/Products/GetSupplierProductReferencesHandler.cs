using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record GetSupplierProductReferencesQuery(
    IEnumerable<int> ProductIds,
    Supplier Supplier
    ) : IQuery<GetSupplierProductReferencesResult>;

public record GetSupplierProductReferencesResult(
    IReadOnlyList<SupplierProductReferenceDto> Products);

public class GetSupplierProductReferencesHandler(
    IReadRepository<Product, int> repository
    ) : IQueryHandler<GetSupplierProductReferencesQuery, GetSupplierProductReferencesResult>
{
    public async Task<GetSupplierProductReferencesResult> Handle(GetSupplierProductReferencesQuery request, CancellationToken cancellationToken)
    {
        var ids = request.ProductIds.Distinct().ToList();
        if (ids.Count == 0) return new GetSupplierProductReferencesResult([]);

        var result = await repository.Query
            .Where(p => ids.Contains(p.Id))
            .SelectMany(
                p => p.Producer.SupplierMappings
                    .Where(m => m.Supplier == request.Supplier)
                    .Take(1),
                (p, m) => new SupplierProductReferenceDto
                {
                    Sku = p.Sku.NormalizedValue,
                    SupplierProducerName = m.SupplierProducerName
                })
            .ToListAsync(cancellationToken);
        
        return new GetSupplierProductReferencesResult(result);
    }
}