namespace Pricing.Application.Models.Pricing;

public record OfferCostScoreContext
{
    public required decimal Cost { get; init; }
    public required int DeliveryDays { get; init; }
    public required int GuaranteedDeliveryDays { get; init; }
}