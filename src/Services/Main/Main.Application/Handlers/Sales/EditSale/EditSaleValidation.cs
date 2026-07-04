using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Sales.BaseValidators;

namespace Main.Application.Handlers.Sales.EditSale;

public class EditSaleValidation : AbstractValidator<EditSaleCommand>
{
    public EditSaleValidation()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithLocalizationKey("sale.id.not.empty");

        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.Content)
            .SetValidator(new EditSaleContentsValidator());
    }
}