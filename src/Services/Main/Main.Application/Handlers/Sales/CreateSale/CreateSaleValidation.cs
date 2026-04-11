using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Sales.BaseValidators;

namespace Main.Application.Handlers.Sales.CreateSale;

public class CreateSaleValidation : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.SellContent)
            .SetValidator(new NewSaleContentValidator());

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);

        RuleFor(x => new { x.SellContent, x.StorageContentValues })
            .Must(x =>
            {
                var arts = x.SellContent.Select(z => z.ArticleId);
                return x.StorageContentValues.All(z => arts.Contains(z.NewValue.ProductId));
            })
            .WithLocalizationKey("sale.articles.missing");
    }
}