using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Auth.AddPermissionsToUser;

public sealed class AddPermissionsToUserDbValidation : AbstractDbValidation<AddPermissionsToUserCommand>
{
	public override void Build(IValidationPlan plan, AddPermissionsToUserCommand request)
	{
		plan.ValidateUserExistsId(request.UserId);
		if (request.PermissionNames.Count > 0)
			plan.ValidatePermissionExistsName(request.PermissionNames);
	}
}
