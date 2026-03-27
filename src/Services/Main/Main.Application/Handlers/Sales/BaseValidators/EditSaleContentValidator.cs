using FluentValidation;
using Localization.Domain.Extensions;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class EditSaleContentValidator : AbstractValidator<EditSaleContentDto>
{
    public EditSaleContentValidator()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithLocalizationKey("sale.content.count.min");

        RuleFor(x => x.Comment)
            .MaximumLength(256)
            .WithLocalizationKey("sale.content.comment.max");

        RuleFor(x => x.Price)
            .SetValidator(new PriceValidator());

        RuleFor(x => x.PriceWithDiscount)
            .SetValidator(new PriceValidator())
            .LessThanOrEqualTo(x => x.Price)
            .WithLocalizationKey("sale.content.price.with.discount.max");
    }
}