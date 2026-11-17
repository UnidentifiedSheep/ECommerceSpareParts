using Contracts.Interfaces;

namespace Pricing.Application.Events;

public record ProductPriceOffersUpdatedEvent : IKeyedEvent
{
	public required int ProductId { get; init; }

	public required string StorageCode { get; init; }

	public string GetKey() => $"product-price-offers-updated:{ProductId}:{StorageCode}";
}
