namespace Integrations.Supplier.Interfaces;

public interface ISupplierFactory
{
    ISupplier Create(Supplier supplier);
}