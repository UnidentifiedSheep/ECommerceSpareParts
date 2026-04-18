using System.Text.Json.Serialization;
using Main.Abstractions.Dtos.Currencies;
using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.StorageRoutes;

public record StorageRouteDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("fromStorageName")]
    public required string FromStorageName { get; init; }
    
    [JsonPropertyName("toStorageName")]
    public required string ToStorageName { get; init; }
    
    [JsonPropertyName("distanceM")]
    public required int DistanceM { get; init; }
    
    [JsonPropertyName("routeType")]
    [JsonConverter(typeof(JsonStringEnumConverter<RouteType>))]
    public required RouteType RouteType { get; init; }
    
    [JsonPropertyName("pricingModel")]
    [JsonConverter(typeof(JsonStringEnumConverter<LogisticPricingType>))]
    public required LogisticPricingType PricingModel { get; init; }
    
    [JsonPropertyName("deliveryTimeMinutes")]
    public required int DeliveryTimeMinutes { get; init; }
    
    [JsonPropertyName("pricePerKg")]
    public required decimal PricePerKg { get; init; }
    
    [JsonPropertyName("pricePerM3")]
    public required decimal PricePerM3 { get; init; }
    
    [JsonPropertyName("pricePerOrder")]
    public required decimal PricePerOrder { get; init; }
    
    [JsonPropertyName("isActive")]
    public required bool IsActive { get; init; }
    
    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }
    
    [JsonPropertyName("minimumPrice")]
    public required decimal? MinimumPrice { get; init; }
    
    [JsonPropertyName("carrierId")]
    public required Guid? CarrierId { get; init; }
}