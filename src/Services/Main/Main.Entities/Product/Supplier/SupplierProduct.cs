using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Entities.Product.ValueObjects;

namespace Main.Entities.Product.Supplier;

public class SupplierProduct : 
    AuditableEntity<SupplierProduct, int>, 
    ILinqEntity<SupplierProduct, int>
{
    private SupplierProduct() { }
    public int Id { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public string Producer { get; private set; } = null!;
    
    private readonly List<SupplierProductName> _names = [];
    public IReadOnlyList<SupplierProductName> Names => _names;
    
    public override int GetId() => Id;
    public static Expression<Func<SupplierProduct, int>> GetKeySelector() => x => x.Id;
    public static Expression<Func<SupplierProduct, bool>> GetEqualityExpression(int key) => x => x.Id == key;
}