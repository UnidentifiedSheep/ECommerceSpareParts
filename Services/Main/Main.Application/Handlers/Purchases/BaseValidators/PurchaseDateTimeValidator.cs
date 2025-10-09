using FluentValidation;

namespace Main.Application.Handlers.Purchases.BaseValidators;

public class PurchaseDateTimeValidator : AbstractValidator<DateTime>
{
    public PurchaseDateTimeValidator()
    {
        RuleFor(x => x)
            .GreaterThanOrEqualTo(DateTime.Now.Date.AddMonths(-3))
            .WithMessage("Дата закупки не может быть более чем трёхмесячной давности.");

        RuleFor(x => x)
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(10))
            .WithMessage("Дата закупки не может быть в будущем.");
    }
}