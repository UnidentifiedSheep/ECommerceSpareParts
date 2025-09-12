using Application.Handlers.Purchases.BaseValidators;
using Application.Handlers.Purchases.BaseValidators.Edit;
using FluentValidation;

namespace Application.Handlers.Purchases.EditFullPurchase;

public class EditFullPurchaseValidation : AbstractValidator<EditFullPurchaseCommand>
{
    public EditFullPurchaseValidation()
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty().WithMessage("Id закупки не может быть пуст");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Содержимое закупки не может быть пустым");

        RuleFor(x => x.PurchaseDateTime)
            .SetValidator(new PurchaseDateTimeValidator());

        RuleFor(x => x.Content)
            .SetValidator(new EditPurchaseDtoValidation());
    }
}