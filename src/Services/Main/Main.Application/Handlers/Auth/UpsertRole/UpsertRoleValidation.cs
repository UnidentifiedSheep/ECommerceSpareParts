using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Auth.UpsertRole;

public class UpsertRoleValidation : AbstractValidator<UpsertRoleCommand>
{
    public UpsertRoleValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithLocalizationKey("role.name.not.empty")
            .Must(x => x.Trim().Length >= 3)
            .WithLocalizationKey("role.name.min.length")
            .Must(x => x.Trim().Length <= 24)
            .WithLocalizationKey("role.name.max.length");
    }
}