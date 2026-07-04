using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public record ProductUpdatedDomainEvent(int Id) : IKeyedDomainEvent, IBatchableDomainEvent
{
    public string GetKey() => $"product:{Id}:updated";
}