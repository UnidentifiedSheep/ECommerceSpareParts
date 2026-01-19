using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;

public class MakeLinkageBetweenArticlesDbValidation : AbstractDbValidation<MakeLinkageBetweenArticlesCommand>
{
    public override void Build(IValidationPlan plan, MakeLinkageBetweenArticlesCommand request)
    {
        plan.ValidateArticleExistsId(request.Linkages
                .SelectMany(x => new[] { x.ArticleId, x.CrossArticleId }));
    }
}