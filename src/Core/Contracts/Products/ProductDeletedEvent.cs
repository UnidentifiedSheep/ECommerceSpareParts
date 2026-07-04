using Abstractions.Interfaces.Events;

namespace Contracts.Products;

public record ProductDeletedEvent : IKeyedEvent
{
    public required int Id { get; init; }

    public string GetKey() { return $"product-deleted:{Id}"; }
}