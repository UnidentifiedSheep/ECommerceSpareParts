using FluentValidation;
using Localization.Domain.Extensions;
using Main.Entities.Auth;
using Role = Enums.Role;

namespace Main.Application.Handlers.Auth.AddRoleToUser;

public class AddRoleToUserValidation : AbstractValidator<AddRoleToUserCommand>
{
    public AddRoleToUserValidation()
    {
        RuleFor(x => x.RoleName)
            .Must(x => RoleNames.Normalize(x) != RoleNames.Normalize(nameof(Role.System)))
            .WithLocalizationKey("cant.add.system.role.to.user");
    }
}