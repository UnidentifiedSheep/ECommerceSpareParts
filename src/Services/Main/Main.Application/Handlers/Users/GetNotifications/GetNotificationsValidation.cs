using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Users.GetNotifications;

public class GetNotificationsValidation : AbstractValidator<GetNotificationsQuery>
{
	public GetNotificationsValidation()
	{
		RuleFor(x => x.Cursor)
			.SetValidator(new CursorValidator<DateTime?>());
	}
}
