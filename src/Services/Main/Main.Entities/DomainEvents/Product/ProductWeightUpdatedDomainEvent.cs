using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public class ProductWeightUpdatedDomainEvent : IKeyedDomainEvent, IBatchableDomainEvent
{
    public int ProductId { get; }
    
    public ProductWeightUpdatedDomainEvent(int productId)
    {
        ProductId = productId;
    }
    
    public string GetKey() => $"product:{ProductId}:weight:updated";
}