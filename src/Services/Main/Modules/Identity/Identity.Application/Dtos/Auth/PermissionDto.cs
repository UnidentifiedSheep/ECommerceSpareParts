using System.Text.Json.Serialization;

namespace Main.Modules.Identity.Application.Dtos.Auth;

public record PermissionDto
{
	[JsonPropertyName("systemName")]
	public required string SystemName { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }
}
