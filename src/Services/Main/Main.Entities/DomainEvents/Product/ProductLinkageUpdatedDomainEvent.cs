using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public record ProductLinkageUpdatedDomainEvent : IKeyedDomainEvent, IBatchableDomainEvent
{

    public ProductLinkageUpdatedDomainEvent(int productId)
    {
        ProductId = productId;
    }
    public int ProductId { get; }

    public string GetKey()
    {
        return $"product:{ProductId}:linkage:updated";
    }
}