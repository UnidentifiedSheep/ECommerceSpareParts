using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;

namespace Pricing.Application.Handlers.Prices.GetPrices;

public class GetPricesValidation : AbstractValidator<GetPricesQuery>
{
    public GetPricesValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}