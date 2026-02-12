using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Permissions.CreatePermission;

public class CreatePermissionDbValidation : AbstractDbValidation<CreatePermissionCommand>
{
    public override void Build(IValidationPlan plan, CreatePermissionCommand request)
    {
        plan.ValidatePermissionNotExistsName(request.Name.ToNormalized());
    }
}