using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Balance.GetOrganizationFinancialInfo;

public class GetOrganizationFinancialInfoDbValidation
    : AbstractDbValidation<GetOrganizationFinancialInfoQuery>
{
    public override void Build(
        IValidationPlan plan,
        GetOrganizationFinancialInfoQuery request)
    {
        plan.ValidateOrganizationExistsId(request.OrganizationId);
    }
}
