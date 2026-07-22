using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.AddOrganizationMember;

public class AddOrganizationMemberDbValidation : AbstractDbValidation<AddOrganizationMemberCommand>
{
    public override void Build(
        IValidationPlan plan,
        AddOrganizationMemberCommand request)
    {
        plan.ValidateOrganizationExistsId(request.OrganizationId)
            .ValidateUserExistsId(request.UserId)
            .ValidateOrganizationMemberNotExistsPK((request.OrganizationId, request.UserId));
    }
}
