using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Enums;
using Main.Enums;

namespace Main.Entities.Producer;

public class ProducerSupplierMapping : Entity<ProducerSupplierMapping, int>, ILinqEntity<ProducerSupplierMapping, int>
{

    private ProducerSupplierMapping() { }
    public int Id { get; private set; }
    public int ProducerId { get; private set; }
    public Supplier Supplier { get; private set; }
    public string SupplierProducerName { get; private set; } = string.Empty;

    public static Expression<Func<ProducerSupplierMapping, int>> GetKeySelector()
    {
        return x => x.Id;
    }

    public static Expression<Func<ProducerSupplierMapping, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public static ProducerSupplierMapping Create(
        int producerId,
        string supplierProducerName,
        Supplier supplier)
    {
        return new ProducerSupplierMapping
        {
            ProducerId = producerId,
            Supplier = supplier,
            SupplierProducerName = supplierProducerName
        };
    }

    public override int GetId()
    {
        return Id;
    }
}