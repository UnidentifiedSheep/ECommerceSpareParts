using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Producer;

public record NewProducerDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}