using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.Price;

public record PriceRecalculationRequestDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("storageName")]
    public required string StorageName { get; init; }
}