using HotChocolate;
using Main.Application.Dtos.NotificationPreference;

namespace Main.Api.GraphQl.Types.Notification;

[GraphQLName("NotificationPreference")]
public record GqlNotificationPreference(
	[property: GraphQLIgnore]
	UserNotificationPreferenceDto Preference)
{
	[GraphQLName("userId")]
	public Guid UserId => Preference.UserId;

	[GraphQLName("channelName")]
	public string ChannelName => Preference.ChannelName;

	[GraphQLName("localizableChannelName")]
	public string LocalizableChannelName => Preference.LocalizableChannelName;

	[GraphQLName("enabled")]
	public bool Enabled => Preference.Enabled;
}
