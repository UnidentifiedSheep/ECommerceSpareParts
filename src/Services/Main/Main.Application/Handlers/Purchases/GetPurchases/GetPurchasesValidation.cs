using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Purchases.GetPurchases;

public class GetPurchasesValidation : AbstractValidator<GetPurchasesQuery>
{
	public GetPurchasesValidation()
	{
		RuleFor(query => query.DateRange)
			.Must(x => !x.Min.HasValue || !x.Max.HasValue || x.Min.Value.Date <= x.Max.Value.Date)
			.WithLocalizationKey("purchase.date.range.start.before.end");

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
