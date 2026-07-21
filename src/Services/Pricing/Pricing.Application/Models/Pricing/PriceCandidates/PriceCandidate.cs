using System.Text.Json.Serialization;
using Pricing.Enums;

namespace Pricing.Application.Models.Pricing.PriceCandidates;

public sealed record PriceCandidate(
    [property: JsonPropertyName("priceOfferId")] Guid PriceOfferId,
    [property: JsonPropertyName("productId")] int ProductId,
    [property: JsonPropertyName("targetStorageName")] string TargetStorageName,

    [property: JsonPropertyName("sourceType")] PriceOfferSourceType SourceType,

    [property: JsonPropertyName("cost")] decimal Cost,
    [property: JsonPropertyName("currencyId")] int CurrencyId,
    
    [property: JsonPropertyName("availableQuantity")] int AvailableQuantity,
    [property: JsonPropertyName("fulfillment")] FulfillmentRouteInfo Fulfillment
);
