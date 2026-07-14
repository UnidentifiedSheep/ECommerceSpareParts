using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Product.Supplier;

public class SupplierProductName : 
    Entity<SupplierProductName, int>,
    ILinqEntity<SupplierProductName, int>
{
    private SupplierProductName() { }
    public int Id { get; private set; }
    public int SupplierProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public global::Enums.Supplier Supplier { get; private set; }

    public SupplierProduct SupplierProduct { get; private set; } = null!;

    public static SupplierProductName Create(
        int supplierProductId,
        string name,
        global::Enums.Supplier supplier)
    {
        return new SupplierProductName
        {
            Supplier = supplier,
            Name = name
                .TrimSafe()
                .AgainstNullOrWhiteSpace(() => new InvalidOperationException(
                    "Supplier product name cannot be null or empty.")),
            SupplierProductId = supplierProductId
        };
    }
    
    public override int GetId() => Id;
    public static Expression<Func<SupplierProductName, int>> GetKeySelector()
        => x => x.Id;
    public static Expression<Func<SupplierProductName, bool>> GetEqualityExpression(int key)
        => x => x.Id == key;
}