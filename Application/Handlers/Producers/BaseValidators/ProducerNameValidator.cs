using FluentValidation;

namespace Application.Handlers.Producers.BaseValidators;

public class ProducerNameValidator : AbstractValidator<string?>
{
    public ProducerNameValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Название производителя не может быть пустым")
            .Must(name => name?.Trim().Length >= 2)
            .WithMessage("Минимальная длина названия производителя — 2 символа")
            .Must(name => name?.Trim().Length <= 64)
            .WithMessage("Максимальная длина названия производителя — 64 символа");

    }
}