using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Product.Supplier;

public class SupplierProductAnalogue : 
    Entity<SupplierProductAnalogue, SupplierProductAnalogueKey>,
    ILinqEntity<SupplierProductAnalogue, SupplierProductAnalogueKey>
{
    private SupplierProductAnalogue() { }
    public int SupplierProductId { get; private set; }
    public int SupplierAnalogueProductId { get; private set; }

    protected SupplierProductAnalogue(
        int supplierProductId,
        int supplierAnalogueProductId)
    {
        if (supplierProductId == supplierAnalogueProductId)
            throw new InvalidOperationException("Supplier product analogue cannot be the same product");
        
        var min = Math.Min(supplierProductId, supplierAnalogueProductId);
        var max = Math.Max(supplierProductId, supplierAnalogueProductId);
        SupplierProductId = min;
        SupplierAnalogueProductId = max;
    }
    
    public static SupplierProductAnalogue Create(
        int supplierProductId,
        int supplierAnalogueProductId)
        => new(supplierProductId, supplierAnalogueProductId);
    
    public override SupplierProductAnalogueKey GetId() 
        => new(SupplierProductId, SupplierAnalogueProductId);
    public static Expression<Func<SupplierProductAnalogue, SupplierProductAnalogueKey>> GetKeySelector()
        => x => new SupplierProductAnalogueKey(x.SupplierProductId, x.SupplierAnalogueProductId);
    public static Expression<Func<SupplierProductAnalogue, bool>> GetEqualityExpression(SupplierProductAnalogueKey key)
        => x => x.SupplierProductId == key.SupplierProductId && x.SupplierAnalogueProductId == key.SupplierAnalogueProductId;
}

public readonly struct SupplierProductAnalogueKey(
    int supplierProductId,
    int supplierAnalogueProductId) : ICompositeKey
{
    public int SupplierProductId => supplierProductId;
    public int SupplierAnalogueProductId => supplierAnalogueProductId;
    public object[] ToArray() => [supplierProductId, supplierAnalogueProductId];
}