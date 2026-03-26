using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Purchases.BaseValidators;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithLocalizationKey("purchase.content.not.empty");

        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .WithLocalizationKey("purchase.supplier.id.not.empty");

        RuleFor(x => x.CreatedUserId)
            .NotEmpty()
            .WithLocalizationKey("purchase.created.user.id.not.empty");

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