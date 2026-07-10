using System.Text.Json.Serialization;
using Pricing.Enums;

namespace Pricing.Application.Dtos.Price;

public record PriceOfferDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("offerForStorage")]
    public required string OfferForStorage { get; init;  }

    [JsonPropertyName("purchasePrice")]
    public required decimal PurchasePrice { get; init; }

    [JsonPropertyName("source")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PriceOfferSource Source { get; init; }

    [JsonPropertyName("availableQuantity")]
    public required int AvailableQuantity { get; init; }
    
    [JsonPropertyName("minimumPurchaseQuantity")]
    public required int MinimumPurchaseQuantity { get; init; }
    
    [JsonPropertyName("quantityCoefficient")]
    public required int QuantityCoefficient { get; init; }

    [JsonPropertyName("daysToRefund")]
    public required int DaysToRefund { get; init; }

    [JsonPropertyName("deliveryDate")]
    public required DateTime? DeliveryDate { get; init; }
    
    [JsonPropertyName("guaranteedDeliveryDate")]
    public required DateTime? GuaranteedDeliveryDate { get; init; }
    
    [JsonPropertyName("deliveryProbability")]
    public required int DeliveryProbability { get; init; }
    
    [JsonPropertyName("orderTill")]
    public required DateTime? OrderTill { get; init; }
    
    [JsonPropertyName("expiresAt")]
    public required DateTime ExpiresAt { get; init; }
}