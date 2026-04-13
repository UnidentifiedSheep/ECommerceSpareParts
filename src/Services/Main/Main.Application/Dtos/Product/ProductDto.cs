using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Amw.Articles;

public record ProductDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("sku")]
    public required string Sku { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string? Description { get; init; }
    
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }
    
    [JsonPropertyName("producerName")]
    public required string ProducerName { get; init; }
    
    [JsonPropertyName("indicator")]
    public required string? Indicator { get; init; }
    
    [JsonPropertyName("images")]
    public List<string> Images { get; init; } = [];
    
    [JsonPropertyName("stock")]
    public required int Stock { get; init; }
}