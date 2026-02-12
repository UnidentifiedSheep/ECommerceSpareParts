using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;

namespace Pricing.Application.Handlers.Prices.GetDetailedPrices;

public class GetDetailedPricesValidation : AbstractValidator<GetDetailedPricesQuery>
{
    public GetDetailedPricesValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}