using System.Text.Json.Serialization;
using Main.Enums.Organization;

namespace Main.Application.Dtos.Organizations;

public record OrganizationDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public required OrganizationType Type { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
}
