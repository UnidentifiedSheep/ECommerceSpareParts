using FluentValidation;
using Localization.Domain.Extensions;

namespace Pricing.Application.Handlers.PriceApplier.DeletePriceApplier;

public class DeletePriceApplierValidation : AbstractValidator<DeletePriceApplierCommand>
{
    public DeletePriceApplierValidation()
    {
        RuleFor(x => x.SystemName)
            .NotEmpty()
            .WithLocalizationKey("price.applier.system.name.required");
    }
}
