using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Sales.BaseValidators;

namespace Main.Application.Handlers.Sales.EditFullSale;

public class EditFullSaleValidation : AbstractValidator<EditFullSaleCommand>
{
    public EditFullSaleValidation()
    {
        RuleFor(x => x.Comment)
            .MaximumLength(256)
            .WithLocalizationKey("sale.comment.max");

        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.EditedContent)
            .SetValidator(new EditSaleContentsValidator());
    }
}