using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleImages.MapImgsToArticle;

public class MapImgsToArticleDbValidation : AbstractDbValidation<MapImgsToArticleCommand>
{
    public override void Build(IValidationPlan plan, MapImgsToArticleCommand request)
    {
        plan.ValidateArticleExistsId(request.ArticleId);
    }
}