using FluentValidation;

namespace Application.Handlers.BuySellPrices.EditBuySellPrices;

public class EditBuySellPricesValidation : AbstractValidator<EditBuySellPricesCommand>
{
    public EditBuySellPricesValidation()
    {
        RuleForEach(x => x.SaleContents)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Price)
                    .PrecisionScale(18, 2, true)
                    .WithMessage("Допустимы только числа с двумя знаками после запятой")
                    .GreaterThan(0)
                    .WithMessage("Цена должна быть больше 0");
            });
    }
}