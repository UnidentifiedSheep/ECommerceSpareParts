using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Purchase;

public record InternalPurchaseLogistic
{
    [JsonPropertyName("routeId")]
    public required Guid RouteId { get; init; }

    [JsonPropertyName("transactionId")]
    public required Guid? TransactionId { get; init; }

    [JsonPropertyName("pricingModel")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required InternalLogisticPricingType PricingModel { get; init; }

    [JsonPropertyName("routeType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required InternalRouteType RouteType { get; init; }

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
    public required InternalCurrency Currency { get; init; }
}