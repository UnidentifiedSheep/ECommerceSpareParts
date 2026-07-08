using Pricing.Enums;

namespace Pricing.Application.Models.Pricing.PriceCandidates;

public record CalculatedPriceCandidate
{
    public required Guid PriceOfferId { get; init; }
    public required int ProductId { get; init; }
    public required string StorageName { get; init; }

    public required PriceOfferSourceType SourceType { get; init; }

    public required decimal CostInBaseCurrency { get; init; }
    public required decimal PriceInBaseCurrency { get; init; }

    public required decimal Markup { get; init; }
    public required int AvailableQuantity { get; init; }

    public required TimeSpan DeliveryTime { get; init; }
    public required TimeSpan GuaranteedDeliveryTime { get; init; }
    public required int DeliveryProbability { get; init; }
}