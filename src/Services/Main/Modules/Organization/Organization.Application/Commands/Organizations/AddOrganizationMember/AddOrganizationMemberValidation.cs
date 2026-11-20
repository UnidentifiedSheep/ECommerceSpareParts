using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Organization;

namespace Main.Modules.Organization.Application.Commands.Organizations.AddOrganizationMember;

public class AddOrganizationMemberValidation : AbstractValidator<AddOrganizationMemberCommand>
{
	public AddOrganizationMemberValidation()
		=> RuleFor(x => x.Role)
			.IsInEnum()
			.WithLocalizableError(OrganizationMemberRoleInvalidMessage.Instance);
}
