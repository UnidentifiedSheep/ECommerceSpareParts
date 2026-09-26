using System.Text.Json.Serialization;

namespace Main.Application.Dtos.NotificationPreference;

public record UpsertUserNotificationPreferenceDto
{
	[JsonPropertyName("channelName")]
	public required string ChannelName { get; init; }

	[JsonPropertyName("isEnabled")]
	public required bool IsEnabled { get; init; }
}
