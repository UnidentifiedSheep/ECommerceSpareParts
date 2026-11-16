using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Organization;
using Main.Modules.Identity.Abstractions.Interfaces;

namespace Main.Modules.Organization.Application.Handlers.Organizations.AddOrganizationMember;

public class AddOrganizationMemberDbValidation(IUserChecks userChecks)
	: AbstractDbValidation<AddOrganizationMemberCommand>
{
	public override void Build(IValidationPlan plan, AddOrganizationMemberCommand request)
	{
		userChecks.Exists(plan, request.UserId);
		plan
			.ValidateOrganizationExistsId(request.OrganizationId)
			.ValidateOrganizationMemberNotExistsPK((request.OrganizationId, request.UserId));
	}
}
