using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Logistics.CalculateDeliveryCost;

public class CalculateDeliveryCostValidation : AbstractValidator<CalculateDeliveryCostQuery>
{
    public CalculateDeliveryCostValidation()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithLocalizationKey("delivery.items.not.empty");

        RuleForEach(x => x.Items)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithLocalizationKey("delivery.item.quantity.greater.than.zero");
            });
    }
}