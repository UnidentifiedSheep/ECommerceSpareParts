using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Core.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Users.AddPermissionToUser;

public class AddPermissionToUserDbValidation : AbstractDbValidation<AddPermissionToUserCommand>
{
    public override void Build(IValidationPlan plan, AddPermissionToUserCommand request)
    {
        plan.ValidateUserExistsId(request.UserId)
            .ValidatePermissionExistsName(request.PermissionName.ToNormalized());
    }
}