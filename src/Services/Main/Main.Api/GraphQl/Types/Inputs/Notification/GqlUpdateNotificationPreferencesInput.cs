using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Notification;

[GraphQLName("UpdateNotificationPreferencesInput")]
public record GqlUpdateNotificationPreferencesInput
{
	[GraphQLName("preferences")]
	public required IReadOnlyList<GqlNotificationPreferencePatchInput> Preferences { get; init; }
}
