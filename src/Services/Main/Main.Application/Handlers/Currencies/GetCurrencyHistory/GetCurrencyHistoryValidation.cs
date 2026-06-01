using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Currencies.GetCurrencyHistory;

public class GetCurrencyHistoryValidation : AbstractValidator<GetCurrencyHistoryQuery>
{
    public GetCurrencyHistoryValidation()
    {
        RuleFor(h => h.Pagination)
            .SetValidator(new PaginationValidator());
    }
}