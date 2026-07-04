using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Auth;

public record PermissionDto
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }
}