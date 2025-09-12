using FluentValidation;

namespace Application.Handlers.BuySellPrices.AddBuySellPrices;

public class AddBuySellPricesValidation : AbstractValidator<AddBuySellPricesCommand>
{
    public AddBuySellPricesValidation()
    {
        RuleForEach(x => x.SaleContents)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Price).GreaterThan(0)
                    .WithMessage("Цена не должна быть отрицательной");
            });
    }
}