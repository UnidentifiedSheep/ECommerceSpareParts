using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;

public class AddContentLogisticsToPurchaseValidation : AbstractValidator<AddContentLogisticsToPurchaseCommand>
{
    public AddContentLogisticsToPurchaseValidation()
    {
        RuleForEach(x => x.Contents)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.AreaM3)
                    .GreaterThanOrEqualTo(0)
                    .WithLocalizationKey("purchase.content.area.min.zero");

                z.RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(0)
                    .WithLocalizationKey("purchase.content.price.min.zero");

                z.RuleFor(x => x.WeightKg)
                    .GreaterThanOrEqualTo(0)
                    .WithLocalizationKey("purchase.content.weight.min.zero");
            });
    }
}