using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities.Purchase;

namespace Main.Modules.Purchase.Application.Handlers.Purchases.GetPurchases;

public class GetPurchasesValidation : AbstractValidator<GetPurchasesQuery>
{
	public GetPurchasesValidation()
	{
		RuleFor(query => query.DateRange)
			.Must(x => !x.Min.HasValue || !x.Max.HasValue || x.Min.Value.Date <= x.Max.Value.Date)
			.WithLocalizableError(PurchaseDateRangeStartBeforeEndMessage.Instance);

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
