using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Organization;
using Main.Modules.Identity.Abstractions.Interfaces;

namespace Main.Modules.Organization.Application.Handlers.Organizations.CreateOrganization;

public class CreateOrganizationDbValidation(IUserChecks userChecks)
	: AbstractDbValidation<CreateOrganizationCommand>
{
	public override void Build(IValidationPlan plan, CreateOrganizationCommand request)
	{
		userChecks.Exists(plan, request.OwnerId);
		plan.ValidateOrganizationNotExistsSystemName(
			Entities.Organization.Organization.Organization.NormalizeSystemName(request.SystemName));
	}
}
