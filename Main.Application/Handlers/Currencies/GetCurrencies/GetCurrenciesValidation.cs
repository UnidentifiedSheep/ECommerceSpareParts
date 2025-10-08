using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public class GetCurrenciesValidation : AbstractValidator<GetCurrenciesQuery>
{
    public GetCurrenciesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}