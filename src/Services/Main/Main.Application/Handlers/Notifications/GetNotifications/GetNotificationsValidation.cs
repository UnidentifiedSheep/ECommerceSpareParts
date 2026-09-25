using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Notifications.GetNotifications;

public class GetNotificationsValidation : AbstractValidator<GetNotificationsQuery>
{
	public GetNotificationsValidation()
	{
		RuleFor(x => x.Cursor)
			.SetValidator(new CursorValidator<DateTime?>());
	}
}
