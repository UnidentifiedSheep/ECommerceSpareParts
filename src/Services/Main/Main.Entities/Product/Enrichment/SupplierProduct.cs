using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.Product.ValueObjects;

namespace Main.Entities.Product.Enrichment;

public class SupplierProduct :
    AuditableEntity<SupplierProduct, int>,
    ILinqEntity<SupplierProduct, int>
{
    private SupplierProduct() { }

    private SupplierProduct(
        Sku sku,
        string producer,
        global::Enums.Supplier supplier)
    {
        Sku = sku;
        Producer = producer
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() =>
                new InvalidOperationException(
                    "Supplier product producer cannot be null or empty."));

        Supplier = supplier;
    }

    public int Id { get; private set; }

    public Sku Sku { get; private set; } = null!;

    public string Producer { get; private set; } = null!;

    public global::Enums.Supplier Supplier { get; private set; }

    public Guid? CatalogueCandidateId { get; private set; }

    public CatalogueCandidate? CatalogueCandidate { get; private set; }

    private readonly List<SupplierProductName> _names = [];
    public IReadOnlyList<SupplierProductName> Names => _names;

    public static SupplierProduct Create(
        Sku sku,
        string producer,
        global::Enums.Supplier supplier)
    {
        return new SupplierProduct(
            sku,
            producer,
            supplier);
    }

    public void AssignToCatalogueCandidate(
        Guid catalogueCandidateId)
    {
        CatalogueCandidateId = catalogueCandidateId;
    }

    public void RemoveFromCatalogueCandidate()
    {
        CatalogueCandidateId = null;
    }

    public void AddName(string name)
    {
        var normalizedName = name
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() =>
                new InvalidOperationException(
                    "Supplier product name cannot be null or empty."));

        if (_names.Any(x =>
                string.Equals(
                    x.Name,
                    normalizedName,
                    StringComparison.OrdinalIgnoreCase)))
            return;

        _names.Add(SupplierProductName.Create(Id, normalizedName));
    }

    public override int GetId() => Id;

    public static Expression<Func<SupplierProduct, int>>
        GetKeySelector()
        => x => x.Id;

    public static Expression<Func<SupplierProduct, bool>>
        GetEqualityExpression(int key)
        => x => x.Id == key;
}
