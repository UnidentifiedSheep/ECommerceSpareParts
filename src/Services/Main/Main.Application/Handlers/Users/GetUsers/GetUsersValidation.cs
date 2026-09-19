using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Users.GetUsers;

public class GetUsersValidation : AbstractValidator<GetUsersQuery>
{
	public GetUsersValidation()
	{
		RuleFor(query => query.Pagination).SetValidator(new PaginationValidator());

		RuleFor(query => query.SimilarityLevel)
			.InclusiveBetween(0, 1)
			.WithLocalizableError(UserSimilarityLevelRangeMessage.Instance);
	}
}
