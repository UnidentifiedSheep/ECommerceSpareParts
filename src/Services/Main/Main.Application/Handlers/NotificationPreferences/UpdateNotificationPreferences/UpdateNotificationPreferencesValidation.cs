using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Notification.Core.Recipients;

namespace Main.Application.Handlers.NotificationPreferences.UpdateNotificationPreferences;

public class UpdateNotificationPreferencesValidation : AbstractValidator<UpdateNotificationPreferencesCommand>
{
	public UpdateNotificationPreferencesValidation()
	{
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
