using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

public class CreatePurchaseDbValidation : AbstractDbValidation<CreatePurchaseCommand>
{
    public override void Build(IValidationPlan plan, CreatePurchaseCommand request)
    {
        plan.ValidateUserExistsId(request.SupplierId)
            .ValidateCurrencyExistsId(request.CurrencyId)
            .ValidateTransactionExistsId(request.TransactionId)
            .ValidateStorageExistsName(request.StorageName)
            .ValidateProductExistsId(request.Content.Select(x => x.content.ProductId).ToHashSet());

        var storageContentIds = request.Content
            .Where(x => x.storageContentId != null)
            .Select(x => x.storageContentId!.Value).ToHashSet();

        if (storageContentIds.Count > 0)
            plan.ValidateStorageContentExistsId(storageContentIds);
    }
}