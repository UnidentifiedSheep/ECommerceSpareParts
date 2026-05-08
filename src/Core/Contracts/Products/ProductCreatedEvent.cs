using Abstractions.Interfaces;

namespace Contracts.Articles;

public record ProductCreatedEvent : IKeyedEvent
{
    public required int Id { get; init; }

    public string GetKey()
    {
        return $"product-created:{Id}";
    }
}