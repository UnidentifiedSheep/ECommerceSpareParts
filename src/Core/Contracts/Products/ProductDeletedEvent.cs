using Abstractions.Interfaces;

namespace Contracts.Articles;

public record ProductDeletedEvent : IKeyedEvent
{
    public required int Id { get; init; }
    public string GetKey() => $"product-deleted:{Id}";
}