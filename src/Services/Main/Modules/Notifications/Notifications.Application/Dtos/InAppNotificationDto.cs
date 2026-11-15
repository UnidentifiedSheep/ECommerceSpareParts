using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Users;

public record InAppNotificationDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("userId")]
	public required Guid UserId { get; init; }

	[JsonPropertyName("text")]
	public required string Text { get; init; }

	[JsonPropertyName("createdAt")]
	public required DateTime CreateAt { get; init; }

	[JsonPropertyName("seenAt")]
	public required DateTime? SeenAt { get; init; }
}
