using Domain.Interfaces.Events;

namespace Pricing.Entities.DomainEvents;

public record PriceApplierUpdatedDomainEvent : IBatchableDomainEvent, IKeyedDomainEvent
{
    public required string SystemName { get; init; }

    public string GetKey() => $"price:applier:{SystemName}:updated";
}
