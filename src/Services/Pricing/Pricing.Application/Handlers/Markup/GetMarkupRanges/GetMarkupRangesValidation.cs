using Application.Common.Validators;
using FluentValidation;

namespace Pricing.Application.Handlers.Markup.GetMarkupRanges;

public sealed class GetMarkupRangesValidation : AbstractValidator<GetMarkupRangesQuery>
{
	public GetMarkupRangesValidation()
	{
		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
