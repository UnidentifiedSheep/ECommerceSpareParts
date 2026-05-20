using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Products;

public record ProductDimensionsDto
{
    [JsonPropertyName("length")]
    public required decimal Length { get; init; }
    
    [JsonPropertyName("width")]
    public required decimal Width { get; init; }
    
    [JsonPropertyName("height")]
    public required decimal Height { get; init; }
    
    [JsonPropertyName("unit")]
    public required string Unit { get; init; }
    
    [JsonPropertyName("volumeM3")]
    public required decimal VolumeM3 { get; init; }
}
