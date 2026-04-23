using System.Text.Json.Serialization;
using Main.Abstractions.Dtos.Amw.Articles;

namespace Main.Abstractions.Dtos.Amw.Sales;

public record SaleContentDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("count")]
    public required int Count { get; set; }
    
    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
    
    [JsonPropertyName("totalSum")]
    public required decimal TotalSum { get; init; }
    
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
    
    [JsonPropertyName("discount")]
    public required decimal Discount { get; init; }
    
    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }
}