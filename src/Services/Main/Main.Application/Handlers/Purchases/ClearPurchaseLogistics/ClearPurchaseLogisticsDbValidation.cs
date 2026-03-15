using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.ClearPurchaseLogistics;

public class ClearPurchaseLogisticsDbValidation : AbstractDbValidation<ClearPurchaseLogisticsCommand>
{
    public override void Build(IValidationPlan plan, ClearPurchaseLogisticsCommand request)
    {
        plan.ValidatePurchaseExistsId(request.PurchaseId);
    }
}