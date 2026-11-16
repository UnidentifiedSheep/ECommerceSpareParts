using Contracts.Models.Supplier;
using Enums;

namespace Main.Modules.Product.Abstractions.Events;

public record SupplierProductsRequestedEvent
{
	public required DateTime OccurredAt { get; init; }

	public required Supplier Supplier { get; init; }

	public required string RequestedStorageCode { get; init; }

	public required IReadOnlyCollection<ContractSupplierProductDto> Products { get; init; }
}
