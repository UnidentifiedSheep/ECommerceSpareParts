using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Organization;

namespace Main.Modules.Organization.Application.Handlers.Organizations.ChangeOrganizationMemberRole;

public class ChangeOrganizationMemberRoleValidation : AbstractValidator<ChangeOrganizationMemberRoleCommand>
{
	public ChangeOrganizationMemberRoleValidation()
		=> RuleFor(x => x.Role)
			.IsInEnum()
			.WithLocalizableError(OrganizationMemberRoleInvalidMessage.Instance);
}
