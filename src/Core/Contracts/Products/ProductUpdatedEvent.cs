using Abstractions.Interfaces;

namespace Contracts.Articles;

public record ProductUpdatedEvent : IKeyedEvent
{
    public required int Id { get; init; }
    public string GetKey() => $"product-updated:{Id}";
}