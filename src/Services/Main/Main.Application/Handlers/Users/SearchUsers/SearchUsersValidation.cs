using Application.Common;
using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Users.SearchUsers;

public sealed class SearchUsersValidation : AbstractValidator<SearchUsersQuery>
{
	public SearchUsersValidation()
	{
		RuleFor(query => query.Pagination)
			.SetValidator(new PaginationValidator());

		RuleFor(query => query.SearchTerm)
			.NotEmpty()
			.WithLocalizableError(SearchStringRequiredMessage.Instance)
			.MaximumLength(200)
			.WithLocalizableError(SearchStringMaxLengthMessage.Create(200));
	}
}
