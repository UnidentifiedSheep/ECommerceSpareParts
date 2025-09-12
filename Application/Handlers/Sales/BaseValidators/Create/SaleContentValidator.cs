using Application.Handlers.BaseValidators;
using Core.Dtos.Amw.Sales;
using FluentValidation;

namespace Application.Handlers.Sales.BaseValidators.Create;

public class SaleContentValidator : AbstractValidator<IEnumerable<NewSaleContentDto>>
{
    public SaleContentValidator()
    {
        RuleForEach(x => x)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Count)
                    .SetValidator(new CountValidator());

                z.RuleFor(x => x.Price)
                    .GreaterThan(0)
                    .WithMessage("Цена у позиции должна быть больше 0")
                    .PrecisionScale(18, 2, true)
                    .WithMessage("Цена может содержать не более двух знаков после запятой");

                z.RuleFor(x => x.PriceWithDiscount)
                    .GreaterThan(0)
                    .WithMessage("Цена со скидкой у позиции должна быть больше 0")
                    .PrecisionScale(18, 2, true)
                    .WithMessage("Цена со скидкой может содержать не более двух знаков после запятой");

                z.RuleFor(x => x.PriceWithDiscount)
                    .LessThanOrEqualTo(x => x.Price)
                    .WithMessage("Цена со скидкой не может быть больше чем цена без скидки");
            });

        RuleFor(x => x)
            .NotEmpty().WithMessage("Список содержимого продажи не должен быть пуст");
    }
}