using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.BaseValidators;

public class LoginValidator : AbstractValidator<string>
{
    public LoginValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithLocalizationKey("login.must.not.be.empty")
            .Must(x => x.Trim().Length >= 5)
            .WithLocalizationKey("login.min.length.5")
            .Must(x => x.Trim().Length <= 36)
            .WithLocalizationKey("login.max.length.36")
            .Must(x => !x.Contains(' '))
            .WithLocalizationKey("login.cannot.contain.spaces");
    }
}