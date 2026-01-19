using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;

namespace Main.Application.Handlers.Prices.GetDetailedPrices;

public class GetDetailedPricesValidation : AbstractValidator<GetDetailedPricesQuery>
{
    public GetDetailedPricesValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}