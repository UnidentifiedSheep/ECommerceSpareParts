using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Auth.AddRolesToUser;

public sealed class AddRolesToUserDbValidation : AbstractDbValidation<AddRolesToUserCommand>
{
	public override void Build(IValidationPlan plan, AddRolesToUserCommand request)
	{
		plan.ValidateUserExistsId(request.UserId);
	}
}
