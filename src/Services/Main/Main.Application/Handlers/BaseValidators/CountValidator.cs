using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.BaseValidators;

public class CountValidator : AbstractValidator<int>
{
    public CountValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithLocalizationKey("position.count.must.be.greater.than.zero");
    }
}