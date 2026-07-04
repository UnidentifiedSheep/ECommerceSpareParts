using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Producers;

public record ProducerDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("aliases")]
    public required IEnumerable<ProducerAlias> Aliases { get; init; }
}