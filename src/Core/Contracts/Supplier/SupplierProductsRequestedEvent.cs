using Contracts.Models.Supplier;

namespace Contracts.Supplier;

public record SupplierProductsRequestedEvent
{
	public required DateTime OccurredAt { get; init; }

	public required Enums.Supplier Supplier { get; init; }

	public required string RequestedStorageCode { get; init; }

	public required IReadOnlyCollection<ContractSupplierProductDto> Products { get; init; }
}
