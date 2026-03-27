using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Purchases.BaseValidators;

public class PurchaseDateTimeValidator : AbstractValidator<DateTime>
{
    public PurchaseDateTimeValidator()
    {
        RuleFor(x => x)
            .GreaterThanOrEqualTo(DateTime.Now.Date.AddMonths(-3))
            .WithLocalizationKey("purchase.datetime.min.3months");

        RuleFor(x => x)
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(10))
            .WithLocalizationKey("purchase.datetime.not.future");
    }
}