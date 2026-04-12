using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleContent.AddArticleContent;

public class AddArticleContentValidation : AbstractValidator<AddArticleContentCommand>
{
    public AddArticleContentValidation()
    {
        RuleForEach(cmd => cmd.Content)
            .Must((parent, kvp) => kvp.Key != parent.ArticleId)
            .WithLocalizationKey("article.content.self.reference.not.allowed");

        RuleForEach(cmd => cmd.Content)
            .Must(kvp => kvp.Value >= 0)
            .WithLocalizationKey("article.content.count.must.be.non.negative");
    }
}