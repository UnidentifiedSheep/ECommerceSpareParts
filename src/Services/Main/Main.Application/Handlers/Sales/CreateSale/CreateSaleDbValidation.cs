using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Sales.CreateSale;

public class CreateSaleDbValidation : AbstractDbValidation<CreateSaleCommand>
{
    public override void Build(IValidationPlan plan, CreateSaleCommand request)
    {
        plan.ValidateTransactionExistsId(request.TransactionId)
            .ValidateProductExistsId(request.SellContent.Select(x => x.ProductId).ToHashSet())
            .ValidateUserExistsId(request.BuyerId)
            .ValidateStorageExistsName(request.Storage);
    }
}