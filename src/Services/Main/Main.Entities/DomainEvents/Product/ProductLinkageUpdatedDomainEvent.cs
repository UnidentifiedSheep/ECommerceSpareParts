using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public record ProductLinkageUpdatedDomainEvent : IKeyedDomainEvent, IBatchableDomainEvent
{
    public int ProductId { get; }

    public ProductLinkageUpdatedDomainEvent(int productId)
    {
        ProductId = productId;
    }
    
    public string GetKey() => $"product:{ProductId}:linkage:updated";
}