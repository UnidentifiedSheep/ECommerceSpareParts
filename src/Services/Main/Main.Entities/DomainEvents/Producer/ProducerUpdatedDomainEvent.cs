using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Producer;

public record ProducerUpdatedDomainEvent(
    int ProducerId) : IKeyedDomainEvent, IBatchableDomainEvent
{
    public string GetKey() => $"producer:{ProducerId}:updated";
}