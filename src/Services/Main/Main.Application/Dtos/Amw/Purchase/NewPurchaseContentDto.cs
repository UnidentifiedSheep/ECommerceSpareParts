using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Amw.Purchase;

public record NewPurchaseContentDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("count")]
    public required int Count { get; init; }
    
    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
    
    [JsonPropertyName("calculateLogistics")]
    public required bool CalculateLogistics { get; init; }
    
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
}