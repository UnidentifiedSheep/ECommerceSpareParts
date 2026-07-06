using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Application.Dtos.Product.SupplierReferences;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record ResolveSupplierProductReferencesQuery(
    Dictionary<Supplier, IEnumerable<SupplierProductReferenceDto>> References
    ) : IQuery<ResolveSupplierProductReferencesResult>;

public record ResolveSupplierProductReferencesResult(
    Dictionary<Supplier, IEnumerable<ResolvedSupplierProductReferenceDto>> Products
    );

public class ResolveSupplierProductReferencesHandler(
    IReadRepository<Product, int> repository
    ) : IQueryHandler<ResolveSupplierProductReferencesQuery, ResolveSupplierProductReferencesResult>
{
    public async Task<ResolveSupplierProductReferencesResult> Handle(ResolveSupplierProductReferencesQuery request, CancellationToken cancellationToken)
    {
        var references = request.References
            .SelectMany(x => x.Value.Select(reference => new
            {
                Supplier = x.Key,
                Reference = reference
            }))
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Reference.Sku) &&
                !string.IsNullOrWhiteSpace(x.Reference.SupplierProducerName))
            .Select(x => new SupplierProductReference(
                x.Supplier,
                Sku.ToNormalized(x.Reference.Sku),
                x.Reference.SupplierProducerName,
                x.Reference.Sku))
            .Distinct()
            .ToList();

        if (references.Count == 0) return new ResolveSupplierProductReferencesResult([]);

        var suppliers = new HashSet<Supplier>();
        var normalizedSkus = new HashSet<string>();
        var producerNames = new HashSet<string>();

        foreach (var @ref in references)
        {
            suppliers.Add(@ref.Supplier);
            normalizedSkus.Add(@ref.Sku);
            producerNames.Add(@ref.SupplierProducerName);
        }

        var referenceKeys = references
            .Select(x => new SupplierProductReferenceKey(
                x.Supplier,
                x.Sku,
                x.SupplierProducerName))
            .ToHashSet();

        var candidates = await repository.Query
            .Where(x => normalizedSkus.Contains(x.Sku.NormalizedValue))
            .SelectMany(
                x =>
                    x.Producer
                        .SupplierMappings
                        .Where(z =>
                            suppliers.Contains(z.Supplier) &&
                            producerNames.Contains(z.SupplierProducerName)),
                (p, m) => new
                {
                    Supplier = m.Supplier,
                    ProductId = p.Id,
                    Sku = p.Sku.NormalizedValue,
                    SupplierProducerName = m.SupplierProducerName
                })
            .ToListAsync(cancellationToken);

        var result = candidates
            .Where(x => referenceKeys.Contains(new SupplierProductReferenceKey(
                x.Supplier,
                x.Sku,
                x.SupplierProducerName)))
            .SelectMany(candidate => references
                .Where(reference =>
                    reference.Supplier == candidate.Supplier &&
                    reference.Sku == candidate.Sku &&
                    reference.SupplierProducerName == candidate.SupplierProducerName)
                .Select(reference => new
                {
                    candidate.Supplier,
                    Product = new ResolvedSupplierProductReferenceDto
                    {
                        ProductId = candidate.ProductId,
                        Sku = reference.OriginalSku ?? candidate.Sku,
                        SupplierProducerName = reference.SupplierProducerName
                    }
                }))
            .GroupBy(x => x.Supplier)
            .ToDictionary(
                x => x.Key,
                x => x.Select(z => z.Product).AsEnumerable());

        return new ResolveSupplierProductReferencesResult(result);
    }

    private readonly record struct SupplierProductReferenceKey(
        Supplier Supplier,
        string Sku,
        string SupplierProducerName);

    private readonly record struct SupplierProductReference(
        Supplier Supplier,
        string Sku,
        string SupplierProducerName,
        string? OriginalSku);
}
