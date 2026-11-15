using BulkValidation.Core.Interfaces;

namespace Main.Modules.Identity.Abstractions;

public interface IUserChecks
{
	IValidationPlan Exists(IValidationPlan plan, Guid userId);

	IValidationPlan Exists(IValidationPlan plan, IEnumerable<Guid> userIds);
}
