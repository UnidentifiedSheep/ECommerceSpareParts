using Domain.Interfaces.Events;

namespace Main.Entities.Product.DomainEvents.Product;

public class ProductSizeUpdatedDomainEvent : IKeyedDomainEvent, IBatchableDomainEvent
{
	public ProductSizeUpdatedDomainEvent(int productId) => ProductId = productId;

	public int ProductId { get; }

	public string GetKey() => $"product:{ProductId}:size:updated";
}
