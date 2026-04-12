using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Services.Articles;

public class CreateProductDto
{
    [JsonPropertyName("sku")]
    public string Sku { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("producerId")]
    public int ProducerId { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("indicator")]
    public string? Indicator { get; set; }
    
    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }
}