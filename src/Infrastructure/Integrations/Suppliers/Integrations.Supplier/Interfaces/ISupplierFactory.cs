namespace Integrations.Supplier.Interfaces;

public interface ISupplierFactory
{
    ISupplier Create(Supplier supplier);
    Task<IReadOnlyList<ISupplier>> GetAvailableSuppliers(
        CancellationToken cancellationToken = default);
}