using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record ProductContentDto
{
    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
    
    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }
}