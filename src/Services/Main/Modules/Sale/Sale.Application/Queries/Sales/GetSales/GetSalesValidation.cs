using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities.Sale;

namespace Main.Modules.Sale.Application.Queries.Sales.GetSales;

public class GetSalesValidation : AbstractValidator<GetSalesQuery>
{
	public GetSalesValidation()
	{
		RuleFor(query => query.Filter.DateRange)
			.Must(x => !x.Min.HasValue || !x.Max.HasValue || x.Min.Value.Date <= x.Max.Value.Date)
			.WithLocalizableError(SaleRangeStartBeforeEndMessage.Instance);

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
