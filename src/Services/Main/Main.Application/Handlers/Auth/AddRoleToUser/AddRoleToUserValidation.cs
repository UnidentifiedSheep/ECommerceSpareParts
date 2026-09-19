using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Auth;
using Role = Enums.Role;

namespace Main.Application.Handlers.Auth.AddRoleToUser;

public class AddRoleToUserValidation : AbstractValidator<AddRoleToUserCommand>
{
	public AddRoleToUserValidation()
	{
		RuleFor(x => x.RoleName)
			.Must(x => RoleNames.Normalize(x) != RoleNames.Normalize(nameof(Role.System)))
			.WithLocalizableError(CantAddSystemRoleToUserMessage.Instance);
	}
}
