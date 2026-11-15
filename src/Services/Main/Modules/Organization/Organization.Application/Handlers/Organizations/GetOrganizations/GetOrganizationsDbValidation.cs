using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Identity.Abstractions;

namespace Main.Application.Handlers.Organizations.GetOrganizations;

public class GetOrganizationsDbValidation(IUserChecks userChecks) : AbstractDbValidation<GetOrganizationsQuery>
{
	public override void Build(IValidationPlan plan, GetOrganizationsQuery request)
	{
		if (request.UserId.HasValue)
			userChecks.Exists(plan, request.UserId.Value);
	}
}
