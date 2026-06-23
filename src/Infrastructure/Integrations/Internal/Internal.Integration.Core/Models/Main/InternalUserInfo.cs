using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main;

public record InternalUserInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("surname")]
    public required string Surname { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
