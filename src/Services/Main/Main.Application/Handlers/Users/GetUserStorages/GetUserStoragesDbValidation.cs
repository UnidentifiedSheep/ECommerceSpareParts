using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Users.GetUserStorages;

public class GetUserStoragesDbValidation : AbstractDbValidation<GetUserStoragesQuery>
{
    public override void Build(IValidationPlan plan, GetUserStoragesQuery request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}