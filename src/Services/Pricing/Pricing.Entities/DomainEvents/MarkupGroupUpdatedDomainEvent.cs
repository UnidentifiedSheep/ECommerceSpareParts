using Domain.Interfaces.Events;

namespace Pricing.Entities.DomainEvents;

public record MarkupGroupUpdatedDomainEvent : IBatchableDomainEvent, IKeyedDomainEvent
{
    public required int Id { get; init; }
    public string GetKey() => $"markup:group:{Id}:updated";
}