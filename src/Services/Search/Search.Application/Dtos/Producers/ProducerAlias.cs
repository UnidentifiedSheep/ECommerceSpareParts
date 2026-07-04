using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Producers;

public record ProducerAlias
{
    [JsonPropertyName("alias")]
    public required string Alias { get; init; }
}