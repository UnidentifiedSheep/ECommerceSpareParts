using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.RemoveOrganizationMember;

public class RemoveOrganizationMemberDbValidation
    : AbstractDbValidation<RemoveOrganizationMemberCommand>
{
    public override void Build(
        IValidationPlan plan,
        RemoveOrganizationMemberCommand request)
    {
        plan.ValidateOrganizationMemberExistsPK((request.OrganizationId, request.UserId));
    }
}
