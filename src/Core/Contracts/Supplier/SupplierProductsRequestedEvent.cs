using Contracts.Models.Supplier;

namespace Contracts.Supplier;

public record SupplierProductsRequestedEvent
{
    public required Enums.Supplier Supplier { get; init; }
    public required IReadOnlyCollection<ContractSupplierProductDto> Products { get; init; }
}