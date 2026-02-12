using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;

namespace Pricing.Application.Handlers.Prices.GetBasePrices;

public class GetBasePricesValidation : AbstractValidator<GetBasePricesQuery>
{
    public GetBasePricesValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);

        RuleFor(x => x.ArticleIds)
            .NotEmpty()
            .WithMessage("Список id для получения цен не может быть пуст");
    }
}