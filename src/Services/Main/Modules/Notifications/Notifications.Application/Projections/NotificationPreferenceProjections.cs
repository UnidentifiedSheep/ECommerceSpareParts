using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Entities.User;
using Main.Modules.Notifications.Application.Dtos.NotificationPreference;

namespace Main.Modules.Notifications.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class UserNotificationPreferenceProjectionProvider(
	IContextualLocalizer localizer
) : ProjectionProviderBase<UserNotificationPreference, UserNotificationPreferenceDto>
{
	public override Expression<Func<UserNotificationPreference, UserNotificationPreferenceDto>> Projection
	{
		get;
	} = x => new UserNotificationPreferenceDto
	{
		UserId = x.UserId,
		ChannelName = x.ChannelName,
		Enabled = x.Enabled,
		LocalizableChannelName = GetLocalized(localizer, x.ChannelName)
	};

	private static string GetLocalized(IContextualLocalizer localizer, string channelName)
		=> localizer.TryGet(new LocalizableMessage($"{channelName}Channel"), out var value)
			? value
			: channelName;
}
