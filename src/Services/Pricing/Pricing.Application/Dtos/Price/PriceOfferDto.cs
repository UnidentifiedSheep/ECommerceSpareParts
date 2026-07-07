using System.Text.Json.Serialization;
using Pricing.Enums;

namespace Pricing.Application.Dtos.Price;

public record PriceOfferDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("offerCurrencyId")]
    public required int OfferCurrencyId { get; init; }
    
    [JsonPropertyName("offerPrice")]
    public required decimal OfferPrice { get; init; }
    
    [JsonPropertyName("offerForStorage")]
    public required string OfferForStorage { get; init; }

    [JsonPropertyName("source")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PriceOfferSource Source { get; init; }
    
    [JsonPropertyName("sourceKey")]
    public required string SourceKey { get; init; }

    [JsonPropertyName("availableQuantity")]
    public required int AvailableQuantity { get; init; }
    
    [JsonPropertyName("minimumPurchaseQuantity")]
    public required int MinimumPurchaseQuantity { get; init; }
    
    [JsonPropertyName("quantityCoefficient")]
    public required int QuantityCoefficient { get; init; }

    [JsonPropertyName("daysToRefund")]
    public required int DaysToRefund { get; init; }

    [JsonPropertyName("deliveryDate")]
    public DateTime DeliveryDate { get; init; }
    
    [JsonPropertyName("guaranteedDeliveryDate")]
    public DateTime GuaranteedDeliveryDate { get; init; }
    
    [JsonPropertyName("deliveryProbability")]
    public int DeliveryProbability { get; init; }
    
    [JsonPropertyName("orderTill")]
    public DateTime OrderTill { get; init; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; init; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; init; }
}