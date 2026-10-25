using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Auth;
using Role = Enums.Role;

namespace Main.Application.Handlers.Auth.RemoveRolesFromUser;

public sealed class RemoveRolesFromUserValidation : AbstractValidator<RemoveRolesFromUserCommand>
{
	public RemoveRolesFromUserValidation()
	{
		RuleForEach(x => x.RoleNames)
			.Must(name => name != RoleNames.Normalize(nameof(Role.System)))
			.WithLocalizableError(CantRemoveSystemRoleFromUserMessage.Instance);
	}
}
