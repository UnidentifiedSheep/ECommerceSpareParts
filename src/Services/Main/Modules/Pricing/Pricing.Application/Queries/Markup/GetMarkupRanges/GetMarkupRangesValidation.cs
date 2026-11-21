using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Pricing.Entities;

namespace Pricing.Application.Queries.Markup.GetMarkupRanges;

public sealed class GetMarkupRangesValidation : AbstractValidator<GetMarkupRangesQuery>
{
	public GetMarkupRangesValidation()
	{
		RuleFor(x => x.GroupId)
			.GreaterThan(0)
			.WithLocalizableError(MarkupGroupIdMustBePositiveMessage.Instance);

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
