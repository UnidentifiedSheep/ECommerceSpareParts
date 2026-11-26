using Pricing.Entities.Enums;

namespace Pricing.Application.Models.Pricing.PriceCandidates;

public record CalculatedPriceCandidate
{
	public required Guid PriceOfferId { get; init; }

	public required int ProductId { get; init; }

	public required string StorageCode { get; init; }

	public required PriceOfferSourceType SourceType { get; init; }

	public required int CurrencyId { get; init; }

	public required decimal Cost { get; init; }

	public required decimal Price { get; init; }

	public required decimal Markup { get; init; }

	public required int AvailableQuantity { get; init; }

	public required int DeliveryDays { get; init; }

	public required int GuaranteedDeliveryDays { get; init; }

	public required int DeliveryProbability { get; init; }
}
