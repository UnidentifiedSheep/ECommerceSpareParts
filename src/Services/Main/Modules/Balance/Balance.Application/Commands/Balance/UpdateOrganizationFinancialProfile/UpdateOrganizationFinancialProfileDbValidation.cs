using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Organization.Abstractions;

namespace Main.Modules.Balance.Application.Commands.Balance.UpdateOrganizationFinancialProfile;

public class UpdateOrganizationFinancialProfileDbValidation(IOrganizationChecks organizationChecks)
	: AbstractDbValidation<UpdateOrganizationFinancialProfileCommand>
{
	public override void Build(IValidationPlan plan, UpdateOrganizationFinancialProfileCommand request)
		=> organizationChecks.Exists(plan, request.OrganizationId);
}
