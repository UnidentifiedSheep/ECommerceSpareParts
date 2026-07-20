using System.Text.Json.Serialization;
using Pricing.Enums;

namespace Pricing.Application.Dtos.PriceApplier;

public record UpsertPriceApplierStateDto
{
    [JsonPropertyName("usage")]
    public required PriceOfferSourceType Usage { get; init; }
    
    [JsonPropertyName("order")]
    public int? Order { get; init; }
    
    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }
}