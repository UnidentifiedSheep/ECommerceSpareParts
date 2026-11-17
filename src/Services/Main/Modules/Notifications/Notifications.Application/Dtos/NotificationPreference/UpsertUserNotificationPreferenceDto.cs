using System.Text.Json.Serialization;

namespace Main.Modules.Notifications.Application.Dtos.NotificationPreference;

public record UpsertUserNotificationPreferenceDto
{
	[JsonPropertyName("channelName")]
	public required string ChannelName { get; init; }

	[JsonPropertyName("isEnabled")]
	public required bool IsEnabled { get; init; }
}
