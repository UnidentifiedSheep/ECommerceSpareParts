using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleContent.SetArticleContentCount;

public class SetArticlesContentCountValidation : AbstractValidator<SetArticlesContentCountCommand>
{
    public SetArticlesContentCountValidation()
    {
        RuleFor(x => x.Count)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("article.content.count.must.be.non.negative");
    }
}