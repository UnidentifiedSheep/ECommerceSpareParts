using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class SaleDateTimeValidator : AbstractValidator<DateTime>
{
    public SaleDateTimeValidator()
    {
        RuleFor(x => x.ToUniversalTime())
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddMonths(-3))
            .WithLocalizationKey("sale.date.min");

        RuleFor(x => x.ToUniversalTime())
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(10))
            .WithLocalizationKey("sale.date.max");
    }
}