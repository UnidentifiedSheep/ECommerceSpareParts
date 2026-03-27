using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.BaseValidators;

public class PriceValidator : AbstractValidator<decimal>
{
    public PriceValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithLocalizationKey("price.must.be.positive");

        RuleFor(x => x)
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("price.max.two.decimal.places");
    }
}