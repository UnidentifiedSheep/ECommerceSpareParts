using Abstractions.Interfaces;

namespace Contracts.Products;

public record ProductLinkageUpdatedEvent : IKeyedEvent
{
    public required int Id { get; init; }

    public string GetKey()
    {
        return $"product-linkage-updated:{Id}";
    }
}