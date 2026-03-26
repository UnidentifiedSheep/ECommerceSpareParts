using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Articles.PatchArticle;

public class PatchArticleValidation : AbstractValidator<PatchArticleCommand>
{
    public PatchArticleValidation()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        // ArticleNumber
        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .NotEmpty()
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithLocalizationKey("article.articleNumber.must.not.be.empty");
        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .Must(x => x != null && x.Trim().Length >= 3)
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithLocalizationKey("article.articleNumber.min.length.3");
        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .Must(x => x != null && x.Trim().Length <= 128)
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithLocalizationKey("article.articleNumber.max.length.128");

        // ArticleName
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .NotEmpty()
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithLocalizationKey("article.name.must.not.be.empty");
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .Must(x => x?.Trim().Length >= 3)
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithLocalizationKey("article.name.min.length.3");
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .Must(x => x?.Trim().Length <= 255)
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithLocalizationKey("article.name.max.length.255");
    }
}