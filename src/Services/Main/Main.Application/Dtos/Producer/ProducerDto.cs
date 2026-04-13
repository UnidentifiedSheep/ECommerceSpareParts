using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Anonymous.Producers;

public record ProducerDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}