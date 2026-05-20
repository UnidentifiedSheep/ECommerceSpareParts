using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Products;

public record ProductDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }
    
    [JsonPropertyName("dimensions")]
    public ProductDimensionsDto? Dimensions { get; init; }
    
    [JsonPropertyName("weight")]
    public ProductWeightDto? Weight { get; init; }
}
