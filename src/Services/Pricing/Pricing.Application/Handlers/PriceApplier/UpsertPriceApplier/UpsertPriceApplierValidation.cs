using FluentValidation;
using Localization.Domain.Extensions;

namespace Pricing.Application.Handlers.PriceApplier.UpsertPriceApplier;

public class UpsertPriceApplierValidation : AbstractValidator<UpsertPriceApplierCommand>
{
    public UpsertPriceApplierValidation()
    {
        RuleFor(x => x.States)
            .Must(states => states
                .Select(x => x.Usage)
                .Distinct()
                .Count() == states.Count)
            .WithLocalizationKey("price.applier.usage.duplicate");
    }
}
