using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class SaleDateTimeValidator : AbstractValidator<DateTime>
{
    public SaleDateTimeValidator()
    {
        RuleFor(x => x.ToUniversalTime())
            .GreaterThanOrEqualTo(DateTime.Now.Date.AddMonths(-3).ToUniversalTime())
            .WithLocalizationKey("sale.date.min");

        RuleFor(x => x.ToUniversalTime())
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(10).ToUniversalTime())
            .WithLocalizationKey("sale.date.max");
    }
}