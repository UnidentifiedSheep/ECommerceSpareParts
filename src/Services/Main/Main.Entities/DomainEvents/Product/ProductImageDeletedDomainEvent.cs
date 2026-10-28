using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public record ProductImageDeletedDomainEvent(int ProductId, string StorageKey) : IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"product:{ProductId}:image:{StorageKey}:deleted";
}
