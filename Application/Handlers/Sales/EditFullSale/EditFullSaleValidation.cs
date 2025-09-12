using Application.Handlers.Sales.BaseValidators;
using Application.Handlers.Sales.BaseValidators.Edit;
using Application.Handlers.Sales.EditSale;
using FluentValidation;

namespace Application.Handlers.Sales.EditFullSale;

public class EditFullSaleValidation : AbstractValidator<EditFullSaleCommand>
{
    public EditFullSaleValidation()
    {
        RuleFor(x => x.Comment)
            .Must(x => x?.Trim().Length <= 256)
            .WithMessage("Максимальная длина общего комментария — 256 символов.");

        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.EditedContent).SetValidator(new SaleContentValidator());
    }
}