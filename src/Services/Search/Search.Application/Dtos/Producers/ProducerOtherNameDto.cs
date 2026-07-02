using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Producers;

public record ProducerOtherNameDto
{
    [JsonPropertyName("alias")]
    public required string Alias { get; init; }
}