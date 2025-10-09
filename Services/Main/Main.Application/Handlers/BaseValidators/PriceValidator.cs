using FluentValidation;

namespace Main.Application.Handlers.BaseValidators;

public class PriceValidator : AbstractValidator<decimal>
{
    public PriceValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithMessage("Цена должна быть больше 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Цена должна иметь максимум 2 числа после запятой.");
    }
}