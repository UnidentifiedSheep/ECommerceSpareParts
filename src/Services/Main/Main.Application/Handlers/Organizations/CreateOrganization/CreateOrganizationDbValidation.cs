using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Organization;

namespace Main.Application.Handlers.Organizations.CreateOrganization;

public class CreateOrganizationDbValidation : AbstractDbValidation<CreateOrganizationCommand>
{
	public override void Build(IValidationPlan plan, CreateOrganizationCommand request)
		=> plan
			.ValidateOrganizationNotExistsSystemName(Organization.NormalizeSystemName(request.SystemName));
}
