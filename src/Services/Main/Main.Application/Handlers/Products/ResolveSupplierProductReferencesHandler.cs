using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Application.Dtos.Product.SupplierReferences;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record ResolveSupplierProductReferencesQuery(
    IEnumerable<SupplierProductReferenceDto> References,
    Supplier Supplier
    ) : IQuery<ResolveSupplierProductReferencesResult>;

public record ResolveSupplierProductReferencesResult(
    IReadOnlyList<ResolvedSupplierProductReferenceDto> Products
    );

public class ResolveSupplierProductReferencesHandler(
    IReadRepository<Product, int> repository
    ) : IQueryHandler<ResolveSupplierProductReferencesQuery, ResolveSupplierProductReferencesResult>
{
    public async Task<ResolveSupplierProductReferencesResult> Handle(ResolveSupplierProductReferencesQuery request, CancellationToken cancellationToken)
    {
        var references = request.References
            .Where(x => !string.IsNullOrWhiteSpace(x.Sku) && !string.IsNullOrWhiteSpace(x.SupplierProducerName))
            .Select(x => new SupplierProductReferenceKey(
                Sku.ToNormalized(x.Sku),
                x.SupplierProducerName))
            .Distinct()
            .ToList();

        if (references.Count == 0) return new ResolveSupplierProductReferencesResult([]);

        var normalizedSkus = new HashSet<string>();
        var producerNames = new HashSet<string>();

        foreach (var @ref in references)
        {
            normalizedSkus.Add(@ref.Sku);
            producerNames.Add(@ref.SupplierProducerName);
        }

        var referenceKeys = references.ToHashSet();

        var candidates = await repository.Query
            .Where(x => normalizedSkus.Contains(x.Sku.NormalizedValue))
            .SelectMany(
                x =>
                    x.Producer
                        .SupplierMappings
                        .Where(z =>
                            z.Supplier == request.Supplier &&
                            producerNames.Contains(z.SupplierProducerName)),
                (p, m) => new ResolvedSupplierProductReferenceDto
                {
                    ProductId = p.Id,
                    Sku = p.Sku.NormalizedValue,
                    SupplierProducerName = m.SupplierProducerName
                })
            .ToListAsync(cancellationToken);

        var result = candidates
            .Where(x => referenceKeys.Contains(new SupplierProductReferenceKey(
                x.Sku,
                x.SupplierProducerName)))
            .ToList();

        return new ResolveSupplierProductReferencesResult(result);
    }

    private readonly record struct SupplierProductReferenceKey(
        string Sku,
        string SupplierProducerName);
}
