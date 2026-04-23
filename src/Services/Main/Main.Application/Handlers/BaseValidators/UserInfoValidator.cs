using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Dtos.Users;

namespace Main.Application.Handlers.BaseValidators;

public class UserInfoValidator : AbstractValidator<UserInfoDto>
{
    public UserInfoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithLocalizationKey("user.name.required")
            .Must(x => x.Trim().Length >= 3)
            .WithLocalizationKey("user.name.min.length")
            .Must(x => !x.Any(char.IsSymbol))
            .WithLocalizationKey("user.name.no.special.chars")
            .Must(x => x.Trim().Length <= 30)
            .WithLocalizationKey("user.name.max.length");

        RuleFor(x => x.Surname)
            .NotEmpty()
            .WithLocalizationKey("user.surname.required")
            .Must(x => x.Trim().Length >= 3)
            .WithLocalizationKey("user.surname.min.length")
            .Must(x => !x.Any(char.IsSymbol))
            .WithLocalizationKey("user.surname.no.special.chars")
            .Must(x => x.Trim().Length <= 30)
            .WithLocalizationKey("user.surname.max.length");

        RuleFor(x => x.Description)
            .Must(x => x == null || x.Trim().Length <= 300)
            .WithLocalizationKey("user.description.max.length");
    }
}