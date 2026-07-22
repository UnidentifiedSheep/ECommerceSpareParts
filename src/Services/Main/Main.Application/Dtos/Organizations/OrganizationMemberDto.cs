using System.Text.Json.Serialization;
using Main.Application.Dtos.Users;
using Main.Enums.Organization;

namespace Main.Application.Dtos.Organizations;

public record OrganizationMemberDto
{
    [JsonPropertyName("organizationId")]
    public required Guid OrganizationId { get; init; }

    [JsonPropertyName("role")]
    public required OrganizationRole Role { get; init; }

    [JsonPropertyName("user")]
    public required UserDto User { get; init; }
}
