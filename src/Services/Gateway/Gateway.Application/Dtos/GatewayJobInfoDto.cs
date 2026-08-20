using System.Text.Json.Serialization;

namespace Gateway.Application.Dtos;

using SchemaGeneration.Abstractions.Models;

public record GatewayJobInfoDto
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("initStateSchema")]
    public required ObjectSchema InitStateSchema { get; init; }
}
