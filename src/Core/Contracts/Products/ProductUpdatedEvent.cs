using Abstractions.Interfaces.Events;

namespace Contracts.Products;

public record ProductUpdatedEvent : IKeyedEvent
{
    public required int Id { get; init; }

    public string GetKey() { return $"product-updated:{Id}"; }
}