using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.GetOrganizations;

public class GetOrganizationsDbValidation : AbstractDbValidation<GetOrganizationsQuery>
{
    public override void Build(
        IValidationPlan plan,
        GetOrganizationsQuery request)
    {
        if (request.UserId.HasValue)
            plan.ValidateUserExistsId(request.UserId.Value);
    }
}
