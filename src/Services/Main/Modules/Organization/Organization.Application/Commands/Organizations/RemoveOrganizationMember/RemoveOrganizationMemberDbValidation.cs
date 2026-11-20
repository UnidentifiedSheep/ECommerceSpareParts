using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Organization;

namespace Main.Modules.Organization.Application.Commands.Organizations.RemoveOrganizationMember;

public class RemoveOrganizationMemberDbValidation : AbstractDbValidation<RemoveOrganizationMemberCommand>
{
	public override void Build(IValidationPlan plan, RemoveOrganizationMemberCommand request)
		=> plan.ValidateOrganizationMemberExistsPK((request.OrganizationId, request.UserId));
}
