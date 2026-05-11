using Abstractions.Interfaces;

namespace Contracts.Products;

public record ProductCreatedEvent : IKeyedEvent
{
    public required int Id { get; init; }

    public string GetKey()
    {
        return $"product-created:{Id}";
    }
}