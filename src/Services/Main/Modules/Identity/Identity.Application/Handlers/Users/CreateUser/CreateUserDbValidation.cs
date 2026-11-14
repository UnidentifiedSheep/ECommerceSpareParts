using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Auth;
using Main.Entities.User.ValueObjects;

namespace Main.Application.Handlers.Users.CreateUser;

public class CreateUserDbValidation : AbstractDbValidation<CreateUserCommand>
{
	public override void Build(IValidationPlan plan, CreateUserCommand request)
	{
		plan.ValidateUserNotExistsUserNameNormalizedValue(UserName.ToNormalized(request.UserName));

		if (request.Emails.Count > 0)
			plan.ValidateUserEmailNotExistsEmail(request.Emails.Select(email => new Email(email.Email)));

		if (request.Roles.Count > 0)
			plan.ValidateRoleExistsName(request.Roles.Select(RoleNames.Normalize).Distinct());
	}
}
