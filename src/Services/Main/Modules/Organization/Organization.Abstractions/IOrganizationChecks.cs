using BulkValidation.Core.Interfaces;

namespace Main.Modules.Organization.Abstractions;

public interface IOrganizationChecks
{
	IValidationPlan Exists(IValidationPlan plan, Guid organizationId);

	IValidationPlan MemberExists(IValidationPlan plan, Guid organizationId, Guid userId);
}
