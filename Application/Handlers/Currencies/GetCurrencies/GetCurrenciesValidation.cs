using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Currencies.GetCurrencies;

public class GetCurrenciesValidation : AbstractValidator<GetCurrenciesQuery>
{
    public GetCurrenciesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}