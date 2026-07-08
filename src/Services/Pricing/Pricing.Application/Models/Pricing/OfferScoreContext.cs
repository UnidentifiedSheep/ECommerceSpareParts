namespace Pricing.Application.Models.Pricing;

public record OfferScoreContext
{
    public required decimal Cost { get; init; }
    public required int DeliveryDays { get; init; }
    public required int GuaranteedDeliveryDays { get; init; }
}