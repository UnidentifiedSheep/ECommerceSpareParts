using Integrations.Supplier.Interfaces;

namespace Integrations.Supplier;

public class SupplierFactory(
    IEnumerable<ISupplier> suppliers
) : ISupplierFactory
{
    private readonly Dictionary<Supplier, ISupplier> _suppliers = suppliers.ToDictionary(s => s.Supplier);
    public ISupplier Create(Supplier supplier) { return _suppliers[supplier]; }
    public async Task<IReadOnlyList<ISupplier>> GetAvailableSuppliers(
        CancellationToken cancellationToken = default)
    {
        var suppliers = _suppliers.Values.ToList();

        var tasks = suppliers
            .Select(x => x.CheckConnectionAsync(cancellationToken))
            .ToList();

        var results = await Task.WhenAll(tasks);

        return suppliers
            .Where((_, i) => results[i].CanUse)
            .ToList();
    }
}