using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public record ProductDeletedDomainEvent(int ProductId) : IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"product:{ProductId}:deleted";
}
