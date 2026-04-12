using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleContent.AddArticleContent;

public class AddArticleContentDbValidation : AbstractDbValidation<AddArticleContentCommand>
{
    public override void Build(IValidationPlan plan, AddArticleContentCommand request)
    {
        var ids = request.Content.Select(x => x.Key).ToHashSet();
        ids.Add(request.ArticleId);
        plan.ValidateArticleExistsId(ids);
    }
}