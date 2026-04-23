using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductCharacteristics.AddCharacteristics;

public class AddCharacteristicsValidation : AbstractValidator<AddCharacteristicsCommand>
{
    public AddCharacteristicsValidation()
    {
        RuleForEach(x => x.Characteristics)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Value)
                    .NotEmpty()
                    .WithLocalizationKey("article.characteristic.value.must.not.be.empty");

                z.RuleFor(x => x.Value)
                    .Must(x => x.Trim().Length >= 3)
                    .WithLocalizationKey("article.characteristic.value.min.length");

                z.RuleFor(x => x.Value)
                    .Must(x => x.Trim().Length <= 128)
                    .WithLocalizationKey("article.characteristic.value.max.length");

                z.RuleFor(x => x.Name)
                    .MaximumLength(128)
                    .WithLocalizationKey("article.characteristic.name.max.length");
            });
    }
}