using Application.Common.Abstractions;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

public class CreatePurchaseDbValidation : AbstractDbValidation<CreatePurchaseCommand>
{
    public override void Build(IValidationPlan plan, CreatePurchaseCommand request)
    {
        plan.ValidateUserExistsId(Quantifier.All, request.CreatedUserId, request.SupplierId)
            .ValidateCurrencyExistsId(request.CurrencyId)
            .ValidateTransactionExistsId(request.TransactionId)
            .ValidateStorageExistsName(request.StorageName)
            .ValidateArticleExistsId(request.Content.Select(x => x.ArticleId).ToHashSet());
    }
}