using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.ChangeOrganizationMemberRole;

public class ChangeOrganizationMemberRoleValidation : AbstractValidator<ChangeOrganizationMemberRoleCommand>
{
	public ChangeOrganizationMemberRoleValidation()
	{
		RuleFor(x => x.Role).IsInEnum().WithLocalizableError(OrganizationMemberRoleInvalidMessage.Instance);
	}
}
