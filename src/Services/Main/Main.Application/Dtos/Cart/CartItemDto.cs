using System.Text.Json.Serialization;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Dtos.Anonymous.Articles;

namespace Main.Abstractions.Dtos.Cart;

public record CartItemDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("count")]
    public required int Count { get; init; }
    
    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }
    
    [JsonPropertyName("updatedAt")]
    public required DateTime? UpdatedAt { get; init; }
    
    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }
}