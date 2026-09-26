using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Notification;

[GraphQLName("UpsertNotificationPreferenceInput")]
public record GqlUpsertNotificationPreferenceInput
{
	[GraphQLName("channelName")]
	public required string ChannelName { get; init; }

	[GraphQLName("isEnabled")]
	public required bool IsEnabled { get; init; }
}
