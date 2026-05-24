using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;
using Main.Enums;

namespace Main.Application.Dtos.Purchase;

public record PurchaseLogisticDto
{
    [JsonPropertyName("routeId")]
    public required Guid RouteId { get; init; }
    
    [JsonPropertyName("transactionId")]
    public required Guid? TransactionId { get; init; }
    
    [JsonPropertyName("pricingModel")]
    [JsonConverter(typeof(JsonStringEnumConverter<LogisticPricingType>))]
    public required LogisticPricingType PricingModel { get; init; }
    
    [JsonPropertyName("routeType")]
    [JsonConverter(typeof(JsonStringEnumConverter<RouteType>))]
    public required RouteType RouteType { get; init; }
    
    [JsonPropertyName("priceKg")]
    public required decimal PriceKg { get; init; }
    
    [JsonPropertyName("pricePerM3")]
    public required decimal PricePerM3 { get; init; }
    
    [JsonPropertyName("pricePerOrder")]
    public required decimal PricePerOrder { get; init; }
    
    [JsonPropertyName("minimumPrice")]
    public required decimal? MinimumPrice { get; init; }
    
    [JsonPropertyName("minimumPriceApplied")]
    public required bool MinimumPriceApplied { get; init; }
    
    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }
}