using Pricing.Enums;

namespace Pricing.Application.Models.Pricing;

public sealed record CalculatedPriceCandidate(
    Guid PriceOfferId,
    int ProductId,
    string StorageName,

    PriceOfferSourceType SourceType,

    decimal CostInBaseCurrency,
    decimal PriceInBaseCurrency,

    decimal Markup,
    int AvailableQuantity,

    TimeSpan DeliveryTime,
    TimeSpan GuaranteedDeliveryTime,
    int DeliveryProbability);