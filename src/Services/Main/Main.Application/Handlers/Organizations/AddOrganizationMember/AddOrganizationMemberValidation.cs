using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.AddOrganizationMember;

public class AddOrganizationMemberValidation : AbstractValidator<AddOrganizationMemberCommand>
{
	public AddOrganizationMemberValidation()
	{
		RuleFor(x => x.Role).IsInEnum().WithLocalizableError(OrganizationMemberRoleInvalidMessage.Instance);
	}
}
