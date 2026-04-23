using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductCharacteristics.PatchCharacteristics;

public class PatchCharacteristicsValidation : AbstractValidator<PatchCharacteristicsCommand>
{
    public PatchCharacteristicsValidation()
    {
        RuleFor(x => x.Patch.Value.Value)
            .NotEmpty()
            .When(x => x.Patch.Value.IsSet)
            .WithLocalizationKey("article.characteristic.value.must.not.be.empty");

        RuleFor(x => x.Patch.Value.Value)
            .MinimumLength(3)
            .When(x => x.Patch.Value.IsSet)
            .WithLocalizationKey("article.characteristic.value.min.length");

        RuleFor(x => x.Patch.Value.Value)
            .MaximumLength(128)
            .When(x => x.Patch.Value.IsSet)
            .WithLocalizationKey("article.characteristic.value.max.length");
    }
}