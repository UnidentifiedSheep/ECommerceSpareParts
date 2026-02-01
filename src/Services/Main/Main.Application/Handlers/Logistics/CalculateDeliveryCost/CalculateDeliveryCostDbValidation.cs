using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Logistics.CalculateDeliveryCost;

public class CalculateDeliveryCostDbValidation : AbstractDbValidation<CalculateDeliveryCostQuery>
{
    public override void Build(IValidationPlan plan, CalculateDeliveryCostQuery request)
    {
        var usableArticleIds = request.Items
            .Select(x => x.ArticleId)
            .ToHashSet();
        plan.ValidateStorageRouteExistsFromTo((request.StorageFrom, request.StorageTo, true))
            .ValidateArticleExistsId(usableArticleIds);
    }
}