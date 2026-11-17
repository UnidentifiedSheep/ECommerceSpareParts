using Domain.Interfaces.Events;

namespace Main.Entities.Product.DomainEvents.Product;

public record ProductImageUpdatedDomainEvent(int ProductId) : IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"product:{ProductId}:image:updated";
}
