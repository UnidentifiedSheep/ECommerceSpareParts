using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Auth;
using Role = Enums.Role;

namespace Main.Application.Handlers.Auth.AddRolesToUser;

public sealed class AddRolesToUserValidation : AbstractValidator<AddRolesToUserCommand>
{
	public AddRolesToUserValidation()
	{
		RuleForEach(x => x.RoleNames)
			.Must(name => name != RoleNames.Normalize(nameof(Role.System)))
			.WithLocalizableError(CantAddSystemRoleToUserMessage.Instance);
	}
}
