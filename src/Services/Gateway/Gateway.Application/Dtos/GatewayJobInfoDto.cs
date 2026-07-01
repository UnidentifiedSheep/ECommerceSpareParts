using System.Text.Json.Serialization;

namespace Gateway.Application.Dtos;

public record GatewayJobInfoDto
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("initStateSchema")]
    public required string InitStateSchema { get; init; }
}