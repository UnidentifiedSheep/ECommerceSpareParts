using Integrations.Supplier.Interfaces;

namespace Integrations.Supplier;

public class SupplierFactory(
    IEnumerable<ISupplier> suppliers
) : ISupplierFactory
{
    private readonly Dictionary<Supplier, ISupplier> _suppliers = suppliers.ToDictionary(s => s.Supplier);
    public ISupplier Create(Supplier supplier) { return _suppliers[supplier]; }
}