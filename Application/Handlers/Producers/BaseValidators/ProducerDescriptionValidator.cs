using FluentValidation;

namespace Application.Handlers.Producers.BaseValidators;

public class ProducerDescriptionValidator : AbstractValidator<string?>
{
    public ProducerDescriptionValidator()
    {
        RuleFor(x => x)
            .Must(desc => desc?.Trim().Length <= 500)
            .When(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Максимальная длина описания — 500 символов");
    }
}