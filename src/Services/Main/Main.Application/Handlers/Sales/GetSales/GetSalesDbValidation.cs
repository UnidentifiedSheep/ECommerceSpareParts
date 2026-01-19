using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Sales.GetSales;

public class GetSalesDbValidation : AbstractDbValidation<GetSalesQuery>
{
    public override void Build(IValidationPlan plan, GetSalesQuery request)
    {
        if (request.BuyerId != null)
            plan.ValidateUserExistsId(request.BuyerId.Value);
    }
}