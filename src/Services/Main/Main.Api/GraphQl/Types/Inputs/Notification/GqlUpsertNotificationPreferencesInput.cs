using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Notification;

[GraphQLName("UpsertNotificationPreferencesInput")]
public record GqlUpsertNotificationPreferencesInput
{
	[GraphQLName("preferences")]
	public required IReadOnlyList<GqlUpsertNotificationPreferenceInput> Preferences { get; init; }
}
