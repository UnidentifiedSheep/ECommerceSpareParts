using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;

public class AddContentLogisticsToPurchaseDbValidation : AbstractDbValidation<AddContentLogisticsToPurchaseCommand>
{
    public override void Build(IValidationPlan plan, AddContentLogisticsToPurchaseCommand request)
    {
        var contentIds = request.Contents.Select(x => x.PurchaseContentId);
        plan.ValidatePurchaseContentExistsId(contentIds);
    }
}