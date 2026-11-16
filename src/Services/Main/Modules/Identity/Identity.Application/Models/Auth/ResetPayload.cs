using System.Text.Json.Serialization;
using Main.Entities.Enums.Auth;

namespace Main.Modules.Identity.Application.Models.Auth;

public record ResetPayload
{
	[JsonPropertyName("id")]
	public Guid Id { get; init; } = Guid.NewGuid();

	[JsonPropertyName("type")]
	public required ResetType Type { get; init; }

	[JsonPropertyName("userId")]
	public required Guid UserId { get; init; }
}
