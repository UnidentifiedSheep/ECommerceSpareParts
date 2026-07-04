using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Producer;

public record ProducerAliasCreatedDomainEvent(
    int ProducerId,
    string Alias) : IBatchableDomainEvent, IKeyedDomainEvent
{
    public string GetKey() => $"producer:{ProducerId}:alias:{Alias}:created";   
}