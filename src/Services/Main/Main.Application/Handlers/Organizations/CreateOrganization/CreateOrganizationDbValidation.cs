using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.CreateOrganization;

public class CreateOrganizationDbValidation : AbstractDbValidation<CreateOrganizationCommand>
{
    public override void Build(
        IValidationPlan plan,
        CreateOrganizationCommand request)
    {
        plan.ValidateUserExistsId(request.OwnerId)
            .ValidateOrganizationNotExistsSystemName(request.SystemName.Trim());
    }
}
