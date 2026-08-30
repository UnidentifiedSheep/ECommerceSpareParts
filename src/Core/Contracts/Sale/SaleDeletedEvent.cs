using Abstractions.Interfaces.Events;

namespace Contracts.Sale;

public record SaleDeletedEvent : IKeyedEvent
{
    public required Guid SaleId { get; init; }
    public required DateTime OccurredAt { get; init; }
    public string GetKey() { return $"sale-deleted:{SaleId}"; }
}
