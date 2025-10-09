using FluentValidation;
using Main.Application.Handlers.Sales.BaseValidators;
using Main.Application.Handlers.Sales.BaseValidators.Create;

namespace Main.Application.Handlers.Sales.CreateSale;

public class CreateSaleValidation : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidation()
    {
        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.SellContent)
            .SetValidator(new SaleContentValidator());

        RuleFor(x => new { x.SellContent, x.StorageContentValues })
            .Must(x =>
            {
                var arts = x.SellContent.Select(z => z.ArticleId);
                return x.StorageContentValues.All(z => arts.Contains(z.NewValue.ArticleId));
            })
            .WithMessage("Не удалось найти Артикул для построения деталей продажи");
    }
}