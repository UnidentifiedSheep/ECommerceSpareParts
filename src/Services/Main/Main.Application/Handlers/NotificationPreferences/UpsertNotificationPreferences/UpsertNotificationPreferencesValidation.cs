using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Notification.Core.Recipients;

namespace Main.Application.Handlers.NotificationPreferences.UpsertNotificationPreferences;

public class UpsertNotificationPreferencesValidation : AbstractValidator<UpsertNotificationPreferencesCommand>
{
	public UpsertNotificationPreferencesValidation()
	{
		RuleFor(x => x.Preferences)
			.Must(preferences =>
				preferences
					.Select(x => x.ChannelName)
					.Distinct()
					.Count() == preferences.Count)
			.WithLocalizableError(NotificationsChannelDuplicateMessage.Instance);

		RuleForEach(x => x.Preferences)
			.ChildRules(z =>
			{
				z.RuleFor(x => x.ChannelName)
					.Must(x => Recipients.All.ContainsKey(x))
					.WithLocalizableError(
						message: NotificationsChannelNotFoundMessage.Create,
						errorCode: NotificationsChannelNotFoundMessage.Key);
			});
	}
}
