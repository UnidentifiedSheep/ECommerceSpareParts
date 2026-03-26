using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.BaseValidators;

public class TransactionAmountValidator : AbstractValidator<decimal>
{
    public TransactionAmountValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithLocalizationKey("transaction.amount.must.be.positive");

        RuleFor(x => x)
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("transaction.amount.max.two.decimal.places");
    }
}