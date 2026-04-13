using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Amw.Producers;

public record NewProducerDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}