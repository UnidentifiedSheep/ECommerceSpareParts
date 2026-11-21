using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Sale.Application.Queries.Sales.GetProductSaleHistory;

public class GetProductSaleHistoryValidation : AbstractValidator<GetProductSaleHistoryQuery>
{
	public GetProductSaleHistoryValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
