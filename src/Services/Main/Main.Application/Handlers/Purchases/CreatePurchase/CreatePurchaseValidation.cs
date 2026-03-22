using FluentValidation;
using Main.Application.Handlers.Purchases.BaseValidators;
using Main.Application.Handlers.Purchases.BaseValidators.Create;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Закупка не может быть пустой");

        RuleFor(x => x.SupplierId).NotEmpty()
            .WithMessage("Id продавца не может быть пустым");
        RuleFor(x => x.CreatedUserId).NotEmpty()
            .WithMessage("Id пользователя создавшего закупку не может быть пустым");

        RuleForEach(x => x.Content)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.content)
                    .SetValidator(new NewPurchaseContentValidation());
            });

        RuleFor(x => x.PurchaseDateTime)
            .SetValidator(new PurchaseDateTimeValidator());
    }
}