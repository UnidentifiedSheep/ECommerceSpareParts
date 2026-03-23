using FluentValidation;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;

public class UpsertPurchaseFactValidation : AbstractValidator<UpsertPurchaseFactCommand>
{
    public UpsertPurchaseFactValidation(IScopedStringLocalizer localizer)
    {
        RuleFor(c => c.PurchaseFact)
            .NotNull()
            .WithMessage(localizer["purchase.fact.required"]);
        
        RuleFor(x => x.PurchaseFact.Content)
            .NotEmpty()
            .WithMessage(localizer["purchase.fact.content.required"]);

        RuleForEach(x => x.PurchaseFact.Content)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Price)
                    .GreaterThan(0)
                    .WithMessage(localizer["purchase.fact.content.price.required"])
                    .PrecisionScale(18, 2, true)
                    .WithMessage(localizer["purchase.fact.content.price.precision.scale"]);

                z.RuleFor(x => x.Count)
                    .GreaterThan(0)
                    .WithMessage(localizer["purchase.fact.content.count.required"]);
            });
    }
}