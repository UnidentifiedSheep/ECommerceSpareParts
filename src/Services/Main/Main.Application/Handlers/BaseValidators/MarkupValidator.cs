using FluentValidation;

namespace Main.Application.Handlers.BaseValidators;

public class MarkupValidator : AbstractValidator<decimal>
{
    public MarkupValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithMessage("Наценка должна быть больше 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Наценка должна иметь максимум 2 числа после запятой.");
    }
}