using Domain.Interfaces.Events;

namespace Main.Entities.Product.DomainEvents.ProductStock;

public record ProductStockUpdatedDomainEvent(string StorageCode, int ProductId)
	: IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"product-stock:{StorageCode}:{ProductId}:updated";
}
