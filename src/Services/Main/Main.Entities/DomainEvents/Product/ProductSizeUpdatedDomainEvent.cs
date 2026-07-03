using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public class ProductSizeUpdatedDomainEvent : IKeyedDomainEvent, IBatchableDomainEvent
{
    public int ProductId { get; }
    
    public ProductSizeUpdatedDomainEvent(int productId)
    {
        ProductId = productId;
    }
    
    public string GetKey() => $"product:{ProductId}:size:updated";
}