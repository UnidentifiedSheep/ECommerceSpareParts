using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleImages.MapImgsToArticle;

public class MapImgsToProductDbValidation : AbstractDbValidation<MapImgsToProductCommand>
{
    public override void Build(IValidationPlan plan, MapImgsToProductCommand request)
    {
        plan.ValidateProductExistsId(request.ProductId);
    }
}