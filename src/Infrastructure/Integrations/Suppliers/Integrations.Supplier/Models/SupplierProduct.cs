namespace Integrations.Supplier.Models;

public class SupplierProduct
{
    public required string Id { get; init; }
    public required string Number { get; init; }
    public required string Brand { get; init; }
    public required string Name { get; init; }

    public required IReadOnlyList<SupplierProduct> Analogues { get; init; } = [];
    public required IReadOnlyList<SupplierPosition> Positions { get; init; } = [];
}