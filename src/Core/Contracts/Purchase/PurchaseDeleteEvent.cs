using Abstractions.Interfaces;

namespace Contracts.Purchase;

public record PurchaseDeleteEvent : IKeyedEvent
{
    public required Guid PurchaseId { get; init; }
    public string GetKey() { return $"purchase-deleted:{PurchaseId}"; }
}