namespace Integrations.Supplier.Interfaces;

public interface ISupplierFactory
{
    ISupplier Create(global::Enums.Supplier supplier);
    Task<IReadOnlyList<ISupplier>> GetAvailableSuppliers(
        CancellationToken cancellationToken = default);
}