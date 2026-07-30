using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;

namespace Main.Entities.Product.Enrichment;

public class SupplierProductName :
    Entity<SupplierProductName, int>,
    ILinqEntity<SupplierProductName, int>
{
    private SupplierProductName() { }

    private SupplierProductName(
        int supplierProductId,
        string name)
    {
        SupplierProductId = supplierProductId;

        Name = name
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() =>
                new InvalidOperationException(
                    "Supplier product name cannot be null or empty."));
    }

    public int Id { get; private set; }

    public int SupplierProductId { get; private set; }

    public string Name { get; private set; } = null!;

    public SupplierProduct SupplierProduct
    { get; private set; } = null!;

    public static SupplierProductName Create(
        int supplierProductId,
        string name)
    {
        return new SupplierProductName(
            supplierProductId,
            name);
    }

    public override int GetId() => Id;

    public static Expression<Func<SupplierProductName, int>>
        GetKeySelector()
        => x => x.Id;

    public static Expression<Func<SupplierProductName, bool>>
        GetEqualityExpression(int key)
        => x => x.Id == key;
}