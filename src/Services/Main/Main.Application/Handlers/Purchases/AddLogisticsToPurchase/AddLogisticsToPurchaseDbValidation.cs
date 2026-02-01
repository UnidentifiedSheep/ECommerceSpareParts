using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.AddLogisticsToPurchase;

public class AddLogisticsToPurchaseDbValidation : AbstractDbValidation<AddLogisticsToPurchaseCommand>
{
    public override void Build(IValidationPlan plan, AddLogisticsToPurchaseCommand request)
    {
        plan.ValidatePurchaseExistsId(request.PurchaseId);
        if (request.TransactionId != null)
            plan.ValidateTransactionExistsId(request.TransactionId.Value);
    }
}