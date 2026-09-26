using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Notification;

[GraphQLName("NotificationPreferencePatchInput")]
public record GqlNotificationPreferencePatchInput
{
	[GraphQLName("channelName")]
	public required string ChannelName { get; init; }

	[GraphQLName("isEnabled")]
	public GqlPatchField<bool>? IsEnabled { get; init; }
}
