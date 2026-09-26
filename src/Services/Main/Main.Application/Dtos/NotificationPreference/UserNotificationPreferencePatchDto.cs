using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Main.Application.Dtos.NotificationPreference;

public record UserNotificationPreferencePatchDto
{
	[JsonPropertyName("channelName")]
	public required string ChannelName { get; init; }

	[JsonPropertyName("isEnable")]
	public PatchField<bool> IsEnabled { get; init; } = PatchField<bool>.NotSet();
}
