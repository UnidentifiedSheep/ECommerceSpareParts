using BulkValidation.Core.Interfaces;
using Main.Entities.Organization;
using Main.Entities;
using Main.Modules.Organization.Abstractions;

namespace Main.Application.Services;

public sealed class OrganizationChecks : IOrganizationChecks
{
	public IValidationPlan Exists(IValidationPlan plan, Guid organizationId)
		=> plan.ValidateOrganizationExistsId(organizationId);

	public IValidationPlan Exists(IValidationPlan plan, IEnumerable<Guid> organizationIds)
		=> plan.ValidateOrganizationExistsId(organizationIds);

	public IValidationPlan MemberExists(IValidationPlan plan, Guid organizationId, Guid userId)
		=> plan.ValidateOrganizationMemberExistsPK((organizationId, userId));
}
