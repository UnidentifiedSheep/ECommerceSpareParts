using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Producer;

public record ProducerAliasDeletedDomainEvent(
    int ProducerId,
    string Alias) : IBatchableDomainEvent, IKeyedDomainEvent 
{
    public string GetKey() => $"producer:{ProducerId}:alias:{Alias}:deleted";  
}