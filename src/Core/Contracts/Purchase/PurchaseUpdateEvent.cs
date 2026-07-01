using Abstractions.Interfaces;

namespace Contracts.Purchase;

public record PurchaseUpdateEvent : IKeyedEvent
{
    public required Guid PurchaseId { get; init; }
    public string GetKey() { return $"purchase-updated:{PurchaseId}"; }
}