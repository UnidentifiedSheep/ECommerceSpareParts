using FluentValidation;

namespace Main.Application.Handlers.Articles.PatchArticle;

public class PatchArticleValidation : AbstractValidator<PatchArticleCommand>
{
    public PatchArticleValidation()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        //ArticleNumber
        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .NotEmpty()
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithMessage("Артикул не должен быть пустым");

        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .Must(x => x != null && x.Trim().Length >= 3)
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithMessage("Минимальная длина артикула 3 символа");

        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .Must(x => x != null && x.Trim().Length <= 128)
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithMessage("Максимальная длина артикула 128 символов");

        //ArticleName
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .NotEmpty()
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithMessage("Название артикула не должен быть пустым");
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .Must(x => x?.Trim().Length > 3)
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithMessage("Минимальная длина название артикула 3 символа");
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .Must(x => x?.Trim().Length <= 255)
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithMessage("Максимальная длина названия артикула 255 символов");
    }
}