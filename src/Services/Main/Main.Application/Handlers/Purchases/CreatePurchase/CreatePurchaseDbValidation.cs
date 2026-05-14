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
            .ValidateProductExistsId(request.Content.Select(x => x.content.ProductId).Distinct());

        var storageContentIds = request.Content
            .Select(x => x.storageContentId)
            .Distinct();

        plan.ValidateStorageContentExistsId(storageContentIds);
    }
}