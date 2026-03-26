using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Producers.BaseValidators;

public class ProducerDescriptionValidator : AbstractValidator<string?>
{
    public ProducerDescriptionValidator()
    {
        RuleFor(x => x)
            .Must(desc => desc?.Trim().Length <= 500)
            .When(x => !string.IsNullOrWhiteSpace(x))
            .WithLocalizationKey("producer.description.max.length");
    }
}