using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Enums.Products;

namespace Main.Entities.Product.Supplier;

public class SupplierProductMapping : 
    AuditableEntity<SupplierProductMapping, int>,
    ILinqEntity<SupplierProductMapping, int>
{
    public int Id { get; private set; }

    public int ProductId { get; private set; }
    public int SupplierProductId { get; private set; }

    public MappingStatus Status { get; private set; }
    public DateTime? LastCheckedAt { get; private set; }
    public override int GetId() => Id;
    public static Expression<Func<SupplierProductMapping, int>> GetKeySelector()
        => x => x.Id;
    public static Expression<Func<SupplierProductMapping, bool>> GetEqualityExpression(int key)
        => x => x.Id == key;
}