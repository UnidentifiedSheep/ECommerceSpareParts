using System.Text.Json.Serialization;

namespace Main.Application.Dtos.NotificationPreference;

public record UserNotificationPreferenceDto
{
	[JsonPropertyName("userId")]
	public required Guid UserId { get; init; }

	[JsonPropertyName("channelName")]
	public required string ChannelName { get; init; }

	[JsonPropertyName("localizableChannelName")]
	public required string LocalizableChannelName { get; init; }

	[JsonPropertyName("enabled")]
	public required bool Enabled { get; init; }
}
