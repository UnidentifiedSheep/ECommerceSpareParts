using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public class GetCurrenciesValidation : AbstractValidator<GetCurrenciesQuery>
{
    public GetCurrenciesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}