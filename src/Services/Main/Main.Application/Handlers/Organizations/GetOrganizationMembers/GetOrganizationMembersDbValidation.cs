using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.GetOrganizationMembers;

public class GetOrganizationMembersDbValidation
    : AbstractDbValidation<GetOrganizationMembersQuery>
{
    public override void Build(
        IValidationPlan plan,
        GetOrganizationMembersQuery request)
    {
        plan.ValidateOrganizationExistsId(request.OrganizationId);
    }
}
