using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Purchases.BaseValidators;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

public class CreateFullPurchaseValidation : AbstractValidator<CreateFullPurchaseCommand>
{
    public CreateFullPurchaseValidation()
    {
        RuleFor(x => x.PurchaseContent)
            .NotEmpty()
            .WithLocalizationKey("purchase.content.not.empty");

        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .WithLocalizationKey("purchase.supplier.id.not.empty");

        RuleFor(x => x.CreatedUserId)
            .NotEmpty()
            .WithLocalizationKey("purchase.created.user.id.not.empty");

        RuleForEach(x => x.PurchaseContent)
            .SetValidator(new NewPurchaseContentValidation());

        RuleFor(x => x.PurchaseDate)
            .SetValidator(new PurchaseDateTimeValidator());

        RuleFor(x => x.StorageFrom)
            .Must(x => x != null)
            .When(x => x.WithLogistics)
            .WithLocalizationKey("purchase.storage.from.required.when.logistics");

        RuleFor(x => x.PayedSum)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PayedSum != null)
            .WithLocalizationKey("purchase.payed.sum.min.value")
            .PrecisionScale(18, 2, true)
            .When(x => x.PayedSum != null)
            .WithLocalizationKey("purchase.payed.sum.precision");
    }
}