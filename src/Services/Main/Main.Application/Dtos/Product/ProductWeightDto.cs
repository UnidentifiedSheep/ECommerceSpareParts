using System.Text.Json.Serialization;
using Enums;

namespace Main.Abstractions.Dtos.ArticleWeight;

public class ProductWeightDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("weight")]
    public required decimal Weight { get; init; }
    
    [JsonPropertyName("unit")]
    [JsonConverter(typeof(JsonStringEnumConverter<WeightUnit>))]
    public required WeightUnit Unit { get; init; }
}