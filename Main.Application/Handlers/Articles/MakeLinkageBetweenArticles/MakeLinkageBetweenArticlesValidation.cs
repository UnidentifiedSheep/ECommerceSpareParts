using FluentValidation;

namespace Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;

public class MakeLinkageBetweenArticlesValidation : AbstractValidator<MakeLinkageBetweenArticlesCommand>
{
    public MakeLinkageBetweenArticlesValidation()
    {
        RuleFor(x => new { x.Linkage.ArticleId, x.Linkage.CrossArticleId })
            .Must(x => x.ArticleId != x.CrossArticleId)
            .WithMessage("Артикул не может быть таким же как кросс артикул");
    }
}