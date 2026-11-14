using Application.Common.Validators;
using FluentValidation;
using Main.Entities.User;

namespace Main.Application.Handlers.Users.GetUsers;

public class GetUsersValidation : AbstractValidator<GetUsersQuery>
{
	public GetUsersValidation()
	{
		RuleFor(query => query.Pagination).SetValidator(new PaginationValidator());
		RuleFor(query => query.Phone)
			.Must(phone => !string.IsNullOrWhiteSpace(UserPhone.ToNormalizedPhone(phone!)))
			.When(query => !string.IsNullOrWhiteSpace(query.Phone));
	}
}
