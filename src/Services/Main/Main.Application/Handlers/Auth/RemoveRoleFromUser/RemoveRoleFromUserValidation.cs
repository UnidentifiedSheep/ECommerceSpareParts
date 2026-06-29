using FluentValidation;
using Localization.Domain.Extensions;
using Main.Entities.Auth;
using Role = Main.Enums.Role;

namespace Main.Application.Handlers.Auth.RemoveRoleFromUser;

public class RemoveRoleFromUserValidation : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserValidation()
    {
        RuleFor(x => x.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithLocalizationKey("role.name.not.empty")
            .Must(x => RoleNames.Normalize(x) != RoleNames.Normalize(nameof(Role.System)))
            .WithLocalizationKey("cant.remove.system.role.from.user");
    }
}
