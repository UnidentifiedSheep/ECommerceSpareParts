using System.Text.Json.Serialization;
using Enums;

namespace Main.Application.Dtos.Amw.Logistics;

public record DeliveryCostItemDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("count")]
    public required decimal Cost { get; init; }
    
    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
    
    [JsonPropertyName("areaM3")]
    public required decimal AreaM3 { get; init; }
    
    [JsonPropertyName("areaPerItem")]
    public required decimal AreaPerItem { get; init; }
    
    [JsonPropertyName("weight")]
    public required decimal Weight { get; init; }
    
    [JsonPropertyName("weightPerItem")]
    public required decimal WeightPerItem { get; init; }
    
    [JsonPropertyName("weightUnit")]
    [JsonConverter(typeof(JsonStringEnumConverter<WeightUnit>))]
    public required WeightUnit WeightUnit { get; init; }
    
    [JsonPropertyName("skipped")]
    public required bool Skipped { get; init; }
    
    [JsonPropertyName("reasons")]
    public required IEnumerable<string>? Reasons { get; init; }
}