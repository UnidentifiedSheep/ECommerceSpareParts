using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleSizes.SetArticleSizes;

public class SetArticleSizesDbValidation : AbstractDbValidation<SetArticleSizesCommand>
{
    public override void Build(IValidationPlan plan, SetArticleSizesCommand request)
    {
        plan.ValidateArticleExistsId(request.ArticleId);
    }
}