using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main;

public record InternalUser
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("userName")]
    public required string UserName { get; init; }

    [JsonPropertyName("normalizedUserName")]
    public required string NormalizedUserName { get; init; }
}