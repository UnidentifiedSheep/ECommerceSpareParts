using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public record ProductStockUpdatedDomainEvent(int Id) : IKeyedDomainEvent, IBatchableDomainEvent
{
    public string GetKey() => $"product:{Id}:stock:updated";
}