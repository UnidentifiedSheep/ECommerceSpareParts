using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.Price;

public record PriceOptionDto
{
    [JsonPropertyName("priceOfferId")]
    public required Guid PriceOfferId { get; init; }
    
    [JsonPropertyName("score")]
    public required decimal Score { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("sellPrice")]
    public required decimal Price { get; init; }
    
    [JsonPropertyName("markup")]
    public required decimal Markup { get; init; }
    
    [JsonPropertyName("forStorage")]
    public required string ForStorageName { get; init; }
    
    [JsonPropertyName("deliveryTime")]
    public required TimeSpan DeliveryTime { get; init; }
    
    [JsonPropertyName("guaranteedDeliveryTime")]
    public required TimeSpan GuaranteedDeliveryTime { get; init; }
    
    [JsonPropertyName("deliveryProbability")]
    public required int DeliveryProbability { get; init; }
    
    [JsonPropertyName("offer")]
    public required PriceOfferDto PriceOffer { get; init; }
}