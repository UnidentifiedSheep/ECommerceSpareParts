using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleReservations.GetArticlesWithNotEnoughStock;

public class GetArticlesWithNotEnoughStockDbValidation : AbstractDbValidation<GetArticlesWithNotEnoughStockQuery>
{
    public override void Build(IValidationPlan plan, GetArticlesWithNotEnoughStockQuery request)
    {
        plan.ValidateStorageExistsName(request.StorageName)
            .ValidateUserExistsId(request.BuyerId)
            .ValidateArticleExistsId(request.NeededCounts.Keys);
    }
}