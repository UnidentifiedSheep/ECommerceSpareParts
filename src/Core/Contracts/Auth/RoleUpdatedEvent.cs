using System.Text.Json.Serialization;

namespace Contracts.Auth;

public record RoleUpdatedEvent
{
    [JsonPropertyName("roleName")]
    public required string RoleName { get; init; }
}