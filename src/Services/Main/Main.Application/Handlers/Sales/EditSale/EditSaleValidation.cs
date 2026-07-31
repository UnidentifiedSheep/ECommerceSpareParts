using Application.Common.Services;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.BaseValidators;
using Main.Application.Handlers.Sales.BaseValidators;

namespace Main.Application.Handlers.Sales.EditSale;

public class EditSaleValidation : AbstractValidator<EditSaleCommand>
{
    public EditSaleValidation(IOperationDatePolicy datePolicy)
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithLocalizationKey("sale.id.not.empty");

        RuleFor(x => x.SaleDateTime)
            .SetValidator(new RecordDateValidator(datePolicy));

        RuleFor(x => x.Content)
            .SetValidator(new EditSaleContentsValidator());
    }
}