using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Producers;

public record ProducerSearchDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
