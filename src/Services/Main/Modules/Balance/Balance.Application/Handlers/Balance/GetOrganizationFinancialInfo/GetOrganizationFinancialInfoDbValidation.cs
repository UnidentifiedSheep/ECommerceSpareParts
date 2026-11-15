using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Organization.Abstractions;

namespace Main.Application.Handlers.Balance.GetOrganizationFinancialInfo;

public class GetOrganizationFinancialInfoDbValidation(IOrganizationChecks organizationChecks)
	: AbstractDbValidation<GetOrganizationFinancialInfoQuery>
{
	public override void Build(IValidationPlan plan, GetOrganizationFinancialInfoQuery request)
		=> organizationChecks.Exists(plan, request.OrganizationId);
}
