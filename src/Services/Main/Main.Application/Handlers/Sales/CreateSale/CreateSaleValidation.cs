using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Sales.BaseValidators;

namespace Main.Application.Handlers.Sales.CreateSale;

public class CreateSaleValidation : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidation()
    {
        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.SellContent)
            .SetValidator(new NewSaleContentValidator());

        RuleFor(x => new { x.SellContent, x.StorageContentValues })
            .Must(x =>
            {
                var arts = x.SellContent.Select(z => z.ProductId);
                return x.StorageContentValues.All(z => arts.Contains(z.ProductId));
            })
            .WithLocalizationKey("sale.articles.missing");
    }
}