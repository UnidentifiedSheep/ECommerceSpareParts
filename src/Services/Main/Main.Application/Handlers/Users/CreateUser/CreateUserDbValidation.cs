using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Users.CreateUser;

public class CreateUserDbValidation : AbstractDbValidation<CreateUserCommand>
{
    public override void Build(IValidationPlan plan, CreateUserCommand request)
    {
        plan.ValidateUserNotExistsNormalizedUserName(request.UserName.ToNormalized());

        var roles = request.Roles.Select(x => x.ToNormalized()).ToList();
        var emails = request.Emails.Select(x => x.Email.Trim().ToNormalizedEmail()).ToList();
        
        if (emails.Count > 0) plan.ValidateUserEmailNotExistsNormalizedEmail(emails);
        if (roles.Count > 0) plan.ValidateRoleExistsNormalizedName(roles);
    }
}