using Domain.Interfaces.Events;

namespace Main.Entities.Product.DomainEvents.Product;

public class ProductWeightUpdatedDomainEvent : IKeyedDomainEvent, IBatchableDomainEvent
{

	public ProductWeightUpdatedDomainEvent(int productId) => ProductId = productId;

	public int ProductId { get; }

	public string GetKey() => $"product:{ProductId}:weight:updated";
}
