using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Balance.UpdateOrganizationFinancialProfile;

public class UpdateOrganizationFinancialProfileDbValidation
    : AbstractDbValidation<UpdateOrganizationFinancialProfileCommand>
{
    public override void Build(
        IValidationPlan plan,
        UpdateOrganizationFinancialProfileCommand request)
    {
        plan.ValidateOrganizationExistsId(request.OrganizationId);
    }
}
