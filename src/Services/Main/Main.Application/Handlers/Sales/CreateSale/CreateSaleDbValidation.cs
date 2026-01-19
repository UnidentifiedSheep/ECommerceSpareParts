using Application.Common.Abstractions;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Sales.CreateSale;

public class CreateSaleDbValidation : AbstractDbValidation<CreateSaleCommand>
{
    public override void Build(IValidationPlan plan, CreateSaleCommand request)
    {
        plan.ValidateTransactionExistsId(request.TransactionId)
            .ValidateArticleExistsId(request.SellContent.Select(x => x.ArticleId).ToHashSet())
            .ValidateUserExistsId(Quantifier.All, request.CreatedUserId, request.BuyerId)
            .ValidateStorageExistsName(request.MainStorage);
    }
}