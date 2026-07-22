using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Organization;

public record InternalOrganization
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required InternalOrganizationType Type { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
}

public enum InternalOrganizationType
{
    Individual,
    Business,
    System
}
