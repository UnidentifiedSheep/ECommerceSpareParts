using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Producer;

public record ProducerDeletedDomainEvent(
    int ProducerId) : IBatchableDomainEvent, IKeyedDomainEvent
{
    public string GetKey() => $"producer:{ProducerId}:deleted";
}