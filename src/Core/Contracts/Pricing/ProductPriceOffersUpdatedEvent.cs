using Abstractions.Interfaces.Events;

namespace Contracts.Pricing;

public record ProductPriceOffersUpdatedEvent : IKeyedEvent
{
    public required int ProductId { get; init; }
    public required string StorageName { get; init; }
    public string GetKey() => $"product-price-offers-updated:{ProductId}:{StorageName}";
}