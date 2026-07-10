using Pricing.Enums;

namespace Pricing.Application.Models.Pricing.PriceCandidates;

public sealed record PriceCandidate(
    Guid PriceOfferId,
    int ProductId,
    string TargetStorageName,

    PriceOfferSourceType SourceType,

    decimal Cost,
    int CurrencyId,
    
    int AvailableQuantity,
    FulfillmentRouteInfo Fulfillment
);