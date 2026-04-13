using System.Text.Json.Serialization;
using Main.Abstractions.Dtos.Amw.Articles;

namespace Main.Abstractions.Dtos.Anonymous.Articles;

public record ProductContentDto
{
    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
    
    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }
}