using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Modules.Identity.Abstractions.Interfaces;

namespace Main.Modules.Identity.Application.Services;

public sealed class UserChecks : IUserChecks
{
	public IValidationPlan Exists(IValidationPlan plan, Guid userId) => plan.ValidateUserExistsId(userId);

	public IValidationPlan Exists(IValidationPlan plan, IEnumerable<Guid> userIds)
		=> plan.ValidateUserExistsId(userIds);
}
