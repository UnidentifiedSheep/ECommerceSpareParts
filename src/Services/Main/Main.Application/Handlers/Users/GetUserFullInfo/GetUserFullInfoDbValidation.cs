using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Users.GetUserFullInfo;

public class GetUserFullInfoDbValidation : AbstractDbValidation<GetUserFullInfoQuery>
{
    public override void Build(IValidationPlan plan, GetUserFullInfoQuery request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}