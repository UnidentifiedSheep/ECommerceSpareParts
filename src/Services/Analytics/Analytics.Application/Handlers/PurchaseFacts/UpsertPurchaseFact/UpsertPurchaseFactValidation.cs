using FluentValidation;
using Localization.Domain.Extensions;

namespace Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;

public class UpsertPurchaseFactValidation : AbstractValidator<UpsertPurchaseFactCommand>
{
    public UpsertPurchaseFactValidation()
    {
        RuleFor(c => c.PurchaseFact)
            .NotNull()
            .WithLocalizationKey("purchase.fact.required");

        RuleFor(x => x.PurchaseFact.Id)
            .NotEmpty()
            .WithLocalizationKey("purchase.fact.id.required");

        RuleFor(x => x.PurchaseFact.Content)
            .NotEmpty()
            .WithLocalizationKey("purchase.fact.content.required");

        RuleForEach(x => x.PurchaseFact.Content)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Price)
                    .GreaterThan(0)
                    .WithLocalizationKey("purchase.fact.content.price.required")
                    .PrecisionScale(18, 2, true)
                    .WithLocalizationKey("purchase.fact.content.price.precision.scale");

                z.RuleFor(x => x.Count)
                    .GreaterThan(0)
                    .WithLocalizationKey("purchase.fact.content.count.required");
            });
    }
}