using FluentValidation;

namespace Application.Handlers.Sales.BaseValidators;

public class SaleDateTimeValidator : AbstractValidator<DateTime>
{
    public SaleDateTimeValidator()
    {
        RuleFor(x => x)
            .GreaterThanOrEqualTo(DateTime.Now.Date.AddMonths(-3))
            .WithMessage("Дата продажи не может быть более чем трёхмесячной давности.");

        RuleFor(x => x)
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(10))
            .WithMessage("Дата продажи не может быть в будущем.");
    }
}