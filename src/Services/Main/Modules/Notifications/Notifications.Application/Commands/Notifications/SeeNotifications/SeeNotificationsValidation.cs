using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Modules.Notifications.Application.Commands.Notifications.SeeNotifications;

public class SeeNotificationsValidation : AbstractValidator<SeeNotificationsCommand>
{
	public SeeNotificationsValidation()
	{
		RuleFor(x => x.Ids)
			.Must(ids => ids.Count <= 100)
			.WithLocalizableError(NotificationsSeeTooManyMessage.Instance);
	}
}
