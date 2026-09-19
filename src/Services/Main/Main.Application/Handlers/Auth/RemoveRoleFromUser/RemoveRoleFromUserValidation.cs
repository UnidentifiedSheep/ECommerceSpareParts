using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Auth;
using Role = Enums.Role;

namespace Main.Application.Handlers.Auth.RemoveRoleFromUser;

public class RemoveRoleFromUserValidation : AbstractValidator<RemoveRoleFromUserCommand>
{
	public RemoveRoleFromUserValidation()
	{
		RuleFor(x => x.RoleName)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizableError(RoleNameNotEmptyMessage.Instance)
			.Must(x => RoleNames.Normalize(x) != RoleNames.Normalize(nameof(Role.System)))
			.WithLocalizableError(CantRemoveSystemRoleFromUserMessage.Instance);
	}
}
