using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.ChangeOrganizationMemberRole;

public class ChangeOrganizationMemberRoleDbValidation
    : AbstractDbValidation<ChangeOrganizationMemberRoleCommand>
{
    public override void Build(
        IValidationPlan plan,
        ChangeOrganizationMemberRoleCommand request)
    {
        plan.ValidateOrganizationMemberExistsPK((request.OrganizationId, request.UserId));
    }
}
