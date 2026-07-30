using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Sales.GetProductSaleHistory;

public class GetProductSaleHistoryValidation : AbstractValidator<GetProductSaleHistoryQuery>
{
    public GetProductSaleHistoryValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}
