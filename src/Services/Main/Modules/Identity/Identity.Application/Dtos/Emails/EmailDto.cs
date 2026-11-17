using System.Text.Json.Serialization;
using Main.Entities.Enums;

namespace Main.Modules.Identity.Application.Dtos.Emails;

public record EmailDto
{
	[JsonPropertyName("email")]
	public required string Email { get; init; }

	[JsonPropertyName("isConfirmed")]
	public bool IsConfirmed { get; init; }

	[JsonPropertyName("isPrimary")]
	public bool IsPrimary { get; init; }

	[JsonPropertyName("type")]
	public EmailType Type { get; init; }
}
