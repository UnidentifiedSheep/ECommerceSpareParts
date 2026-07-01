using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Producers;

public record ProducerOtherNameDto
{
    [JsonPropertyName("otherName")]
    public required string OtherName { get; init; }

    [JsonPropertyName("whereUsed")]
    public required string WhereUsed { get; init; }
}