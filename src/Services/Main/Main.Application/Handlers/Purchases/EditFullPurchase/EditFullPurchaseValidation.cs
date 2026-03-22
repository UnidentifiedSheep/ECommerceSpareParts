using FluentValidation;
using Main.Application.Handlers.Purchases.BaseValidators;
using Main.Application.Handlers.Purchases.BaseValidators.Edit;

namespace Main.Application.Handlers.Purchases.EditFullPurchase;

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
        
        RuleFor(x => x.StorageFrom)
            .Must(x => x != null)
            .When(x => x.WithLogistics)
            .WithMessage("При создании закупки с логистикой, склад отправителя должен быть указан");
    }
}