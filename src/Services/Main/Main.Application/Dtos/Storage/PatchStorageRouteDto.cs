using System.Text.Json.Serialization;
using Abstractions.Models;
using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.StorageRoutes;

public record PatchStorageRouteDto
{
    [JsonPropertyName("distanceM")]
    public PatchField<int> DistanceM { get; init; } = PatchField<int>.NotSet();

    [JsonPropertyName("routeType")]
    public PatchField<RouteType> RouteType { get; init; } = PatchField<RouteType>.NotSet();

    [JsonPropertyName("pricingModel")]
    public PatchField<LogisticPricingType> PricingModel { get; init; } = PatchField<LogisticPricingType>.NotSet();

    [JsonPropertyName("deliveryTimeMinutes")]
    public PatchField<int> DeliveryTimeMinutes { get; init; } = PatchField<int>.NotSet();

    [JsonPropertyName("priceKg")]
    public PatchField<decimal> PriceKg { get; init; } = PatchField<decimal>.NotSet();

    [JsonPropertyName("pricePerM3")]
    public PatchField<decimal> PricePerM3 { get; init; } = PatchField<decimal>.NotSet();

    [JsonPropertyName("pricePerOrder")]
    public PatchField<decimal> PricePerOrder { get; init; } = PatchField<decimal>.NotSet();

    [JsonPropertyName("isActive")]
    public PatchField<bool> IsActive { get; init; } = PatchField<bool>.NotSet();

    [JsonPropertyName("currencyId")]
    public PatchField<int> CurrencyId { get; init; } = PatchField<int>.NotSet();

    [JsonPropertyName("minimumPrice")]
    public PatchField<decimal?> MinimumPrice { get; init; } = PatchField<decimal?>.NotSet();
    
    [JsonPropertyName("carrierId")]
    public PatchField<Guid?> CarrierId { get; init; } = PatchField<Guid?>.NotSet();
}