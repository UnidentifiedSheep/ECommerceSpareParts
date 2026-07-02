using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Enums;

namespace Main.Entities.Producer;

public class SupplierProducerMapping : Entity<SupplierProducerMapping, int>, ILinqEntity<SupplierProducerMapping, int>
{
    public int Id { get; private set; }
    public int ProducerId { get; private set; }
    public Supplier Supplier { get; private set; }
    public string SupplierProducerName { get; private set; } = string.Empty;
    
    private SupplierProducerMapping() { }

    public static SupplierProducerMapping Create(
        int producerId,
        string supplierProducerName,
        Supplier supplier) =>
        new()
        {
            ProducerId = producerId,
            Supplier = supplier,
            SupplierProducerName = supplierProducerName
        };

    public override int GetId() => Id;

    public static Expression<Func<SupplierProducerMapping, int>> GetKeySelector() 
        => x => x.Id;

    public static Expression<Func<SupplierProducerMapping, bool>> GetEqualityExpression(int key)
        => x => x.Id == key;
}