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

    [JsonPropertyName("otherNames")]
    public required IEnumerable<ProducerOtherNameDto> OtherNames { get; init; }
}