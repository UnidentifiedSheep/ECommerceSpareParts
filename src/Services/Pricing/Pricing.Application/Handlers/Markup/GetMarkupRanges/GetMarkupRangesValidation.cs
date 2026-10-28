using Application.Common.Extensions;
using Application.Common.Validators;
using Pricing.Entities;
using FluentValidation;

namespace Pricing.Application.Handlers.Markup.GetMarkupRanges;

public sealed class GetMarkupRangesValidation : AbstractValidator<GetMarkupRangesQuery>
{
	public GetMarkupRangesValidation()
	{
		RuleFor(x => x.GroupId).GreaterThan(0)
			.WithLocalizableError(MarkupGroupIdMustBePositiveMessage.Instance);

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
