using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;

namespace Main.Application.Handlers.Prices.GetBasePrices;

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