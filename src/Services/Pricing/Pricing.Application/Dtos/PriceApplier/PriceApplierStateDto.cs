using System.Text.Json.Serialization;
using Pricing.Enums;

namespace Pricing.Application.Dtos.PriceApplier;

public record PriceApplierStateDto
{
    [JsonPropertyName("priceApplierSystemName")]
    public required string PriceApplierSystemName { get; init; }
    
    [JsonPropertyName("usage")]
    public required PriceOfferSourceType Usage { get; init; }
    
    [JsonPropertyName("order")]
    public required int Order { get; init; }
    
    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }
}