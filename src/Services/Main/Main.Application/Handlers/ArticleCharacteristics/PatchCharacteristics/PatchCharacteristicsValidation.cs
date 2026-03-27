using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleCharacteristics.PatchCharacteristics;

public class PatchCharacteristicsValidation : AbstractValidator<PatchCharacteristicsCommand>
{
    public PatchCharacteristicsValidation()
    {
        RuleFor(x => x.NewValues.Value.Value)
            .NotEmpty()
            .When(x => x.NewValues.Value.IsSet)
            .WithLocalizationKey("article.characteristic.value.must.not.be.empty");

        RuleFor(x => x.NewValues.Value.Value)
            .MinimumLength(3)
            .When(x => x.NewValues.Value.IsSet)
            .WithLocalizationKey("article.characteristic.value.min.length");

        RuleFor(x => x.NewValues.Value.Value)
            .MaximumLength(128)
            .When(x => x.NewValues.Value.IsSet)
            .WithLocalizationKey("article.characteristic.value.max.length");

        RuleFor(x => x.NewValues.Name.Value)
            .MaximumLength(128)
            .When(x => x.NewValues.Name.IsSet)
            .WithLocalizationKey("article.characteristic.name.max.length");
    }
}