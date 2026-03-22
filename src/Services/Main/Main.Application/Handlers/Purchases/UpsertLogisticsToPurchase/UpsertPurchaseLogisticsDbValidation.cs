using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;

public class UpsertPurchaseLogisticsDbValidation : AbstractDbValidation<UpsertPurchaseLogisticsCommand>
{
    public override void Build(IValidationPlan plan, UpsertPurchaseLogisticsCommand request)
    {
        plan.ValidatePurchaseExistsId(request.PurchaseId);
        if (request.TransactionId != null)
            plan.ValidateTransactionExistsId(request.TransactionId.Value);
    }
}