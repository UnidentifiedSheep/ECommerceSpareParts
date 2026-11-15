using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Notifications.SeeNotifications;

public class SeeNotificationsValidation : AbstractValidator<SeeNotificationsCommand>
{
	public SeeNotificationsValidation()
	{
		RuleFor(x => x.Ids)
			.Must(ids => ids.Count <= 100)
			.WithLocalizableError(NotificationsSeeTooManyMessage.Instance);
	}
}
