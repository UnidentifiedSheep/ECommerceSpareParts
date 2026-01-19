using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;

namespace Main.Application.Handlers.Prices.GetPrices;

public class GetPricesValidation : AbstractValidator<GetPricesQuery>
{
    public GetPricesValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}