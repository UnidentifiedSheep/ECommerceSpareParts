using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;

public class
    GetProductsWithNotEnoughStockDbValidation : AbstractDbValidation<GetProductsWithNotEnoughStockQuery>
{
    public override void Build(IValidationPlan plan, GetProductsWithNotEnoughStockQuery request)
    {
        plan.ValidateStorageExistsName(request.StorageName)
            .ValidateUserExistsId(request.BuyerId)
            .ValidateProductExistsId(request.NeededCounts.Keys);
    }
}