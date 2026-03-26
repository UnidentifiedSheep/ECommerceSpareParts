using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

public class DeletePurchaseValidation : AbstractValidator<DeletePurchaseCommand>
{
    public DeletePurchaseValidation()
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty()
            .WithLocalizationKey("purchase.id.not.empty");
    }
}