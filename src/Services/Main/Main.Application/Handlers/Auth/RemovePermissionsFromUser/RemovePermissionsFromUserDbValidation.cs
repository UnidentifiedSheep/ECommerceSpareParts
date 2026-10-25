using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Auth.RemovePermissionsFromUser;

public sealed class RemovePermissionsFromUserDbValidation : AbstractDbValidation<RemovePermissionsFromUserCommand>
{
	public override void Build(IValidationPlan plan, RemovePermissionsFromUserCommand request)
	{
		plan.ValidateUserExistsId(request.UserId);
	}
}
