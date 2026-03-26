using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Producers.BaseValidators;

public class ProducerNameValidator : AbstractValidator<string?>
{
    public ProducerNameValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithLocalizationKey("producer.name.not.empty")
            .Must(name => name?.Trim().Length >= 2)
            .WithLocalizationKey("producer.name.min.length")
            .Must(name => name?.Trim().Length <= 64)
            .WithLocalizationKey("producer.name.max.length");
    }
}