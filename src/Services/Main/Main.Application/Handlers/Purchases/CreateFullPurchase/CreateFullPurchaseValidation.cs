using FluentValidation;
using Main.Application.Handlers.Purchases.BaseValidators;
using Main.Application.Handlers.Purchases.BaseValidators.Create;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

public class CreateFullPurchaseValidation : AbstractValidator<CreateFullPurchaseCommand>
{
    public CreateFullPurchaseValidation()
    {
        RuleFor(x => x.PurchaseContent)
            .NotEmpty()
            .WithMessage("Закупка не может быть пустой");

        RuleFor(x => x.SupplierId).NotEmpty()
            .WithMessage("Id продавца не может быть пустым");
        RuleFor(x => x.CreatedUserId).NotEmpty()
            .WithMessage("Id пользователя создавшего закупку не может быть пустым");

        RuleForEach(x => x.PurchaseContent)
            .SetValidator(new NewPurchaseContentValidation());

        RuleFor(x => x.PurchaseDate)
            .SetValidator(new PurchaseDateTimeValidator());
        
        RuleFor(x => x.StorageFrom)
            .Must(x => x != null)
            .When(x => x.WithLogistics)
            .WithMessage("При создании закупки с логистикой, склад отправителя должен быть указан");

        RuleFor(x => x.PayedSum)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PayedSum != null)
            .WithMessage("Оплаченная сумма должна быть больше или равна 0.")
            .PrecisionScale(18, 2, true)
            .When(x => x.PayedSum != null)
            .WithMessage("Оплаченная сумма должна иметь максимум 2 числа после запятой.");
    }
}