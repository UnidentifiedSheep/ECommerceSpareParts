using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Amw.Articles;

public class ProductDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sku")]
    public string Sku { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("producerId")]
    public int ProducerId { get; set; }
    
    [JsonPropertyName("producerName")]
    public string ProducerName { get; set; } = null!;
    
    [JsonPropertyName("indicator")]
    public string? Indicator { get; set; }
    
    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = [];
    
    [JsonPropertyName("stock")]
    public int Stock { get; set; }
}