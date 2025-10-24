using FluentValidation;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class SaleDateTimeValidator : AbstractValidator<DateTime>
{
    public SaleDateTimeValidator()
    {
        RuleFor(x => x.ToUniversalTime())
            .GreaterThanOrEqualTo(DateTime.Now.Date.AddMonths(-3).ToUniversalTime())
            .WithMessage("Дата продажи не может быть более чем трёхмесячной давности.");

        RuleFor(x => x.ToUniversalTime())
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(10).ToUniversalTime())
            .WithMessage("Дата продажи не может быть в будущем.");
    }
}