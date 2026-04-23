using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Dtos.Amw.Sales;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class NewSaleContentValidator : AbstractValidator<IEnumerable<NewSaleContentDto>>
{
    public NewSaleContentValidator()
    {
        RuleForEach(x => x)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Count)
                    .SetValidator(new CountValidator());

                z.RuleFor(x => x.Price)
                    .GreaterThan(0)
                    .WithLocalizationKey("sale.content.price.min")
                    .PrecisionScale(18, 2, true)
                    .WithLocalizationKey("sale.content.price.precision");

                z.RuleFor(x => x.PriceWithDiscount)
                    .GreaterThan(0)
                    .WithLocalizationKey("sale.content.price.with.discount.min")
                    .PrecisionScale(18, 2, true)
                    .WithLocalizationKey("sale.content.price.with.discount.precision");

                z.RuleFor(x => x.PriceWithDiscount)
                    .LessThanOrEqualTo(x => x.Price)
                    .WithLocalizationKey("sale.content.price.with.discount.max");
            });

        RuleFor(x => x)
            .NotEmpty()
            .WithLocalizationKey("sale.content.list.not.empty");
    }
}