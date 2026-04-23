using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Producer;

public record ProducerDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}