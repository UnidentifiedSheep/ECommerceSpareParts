using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Auth.RemoveRolesFromUser;

public sealed class RemoveRolesFromUserDbValidation : AbstractDbValidation<RemoveRolesFromUserCommand>
{
	public override void Build(IValidationPlan plan, RemoveRolesFromUserCommand request)
	{
		plan.ValidateUserExistsId(request.UserId);
	}
}
