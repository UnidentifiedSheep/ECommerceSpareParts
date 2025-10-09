using FluentValidation;

namespace Main.Application.Handlers.BaseValidators;

public class CountValidator : AbstractValidator<int>
{
    public CountValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithMessage("Количество у позиции должно быть больше 0");
    }
}