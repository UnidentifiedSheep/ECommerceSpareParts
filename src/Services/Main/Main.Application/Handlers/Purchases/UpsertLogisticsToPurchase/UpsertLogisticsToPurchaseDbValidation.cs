using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;

public class UpsertLogisticsToPurchaseDbValidation : AbstractDbValidation<UpsertLogisticsToPurchaseCommand>
{
    public override void Build(IValidationPlan plan, UpsertLogisticsToPurchaseCommand request)
    {
        plan.ValidatePurchaseExistsId(request.PurchaseId);
        if (request.TransactionId != null)
            plan.ValidateTransactionExistsId(request.TransactionId.Value);
    }
}