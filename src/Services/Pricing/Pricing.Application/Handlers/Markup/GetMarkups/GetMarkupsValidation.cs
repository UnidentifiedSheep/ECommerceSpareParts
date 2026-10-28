using Application.Common;
using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;

namespace Pricing.Application.Handlers.Markup.GetMarkups;

public class GetMarkupsValidation : AbstractValidator<GetMarkupsQuery>
{
	public GetMarkupsValidation()
	{
		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
		RuleFor(x => x.SearchTerm).MaximumLength(128)
			.WithLocalizableError(SearchStringMaxLengthMessage.Create(128));
	}
}
