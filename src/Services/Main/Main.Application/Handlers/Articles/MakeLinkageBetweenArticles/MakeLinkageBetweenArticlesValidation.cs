using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;

public class MakeLinkageBetweenArticlesValidation : AbstractValidator<MakeLinkageBetweenArticlesCommand>
{
    public MakeLinkageBetweenArticlesValidation()
    {
        RuleForEach(x => x.Linkages)
            .ChildRules(z =>
            {
                z.RuleFor(x => new { x.ArticleId, x.CrossArticleId })
                    .Must(x => x.ArticleId != x.CrossArticleId)
                    .WithLocalizationKey("article.linkage.article.cannot.equal.cross.article");
            });
    }
}