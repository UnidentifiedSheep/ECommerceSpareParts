using Abstractions.Interfaces.Events;

namespace Contracts.Purchase;

public record PurchaseDeleteEvent : IKeyedEvent
{
	public required Guid PurchaseId { get; init; }

	public string GetKey() => $"purchase-deleted:{PurchaseId}";
}
