using Abstractions.Interfaces.Localization;
using FluentValidation;

namespace Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;

public class UpsertPurchaseFactValidation : AbstractValidator<UpsertPurchaseFactCommand>
{
    public UpsertPurchaseFactValidation(IScopedStringLocalizer localizer)
    {
        RuleFor(c => c.PurchaseFact)
            .NotNull()
            .WithMessage(localizer["purchase.fact.required"]);
    }
}