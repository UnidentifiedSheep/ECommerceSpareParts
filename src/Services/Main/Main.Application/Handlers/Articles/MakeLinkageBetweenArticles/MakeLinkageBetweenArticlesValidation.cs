using FluentValidation;

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
                    .WithMessage("Артикул не может быть таким же как кросс артикул");
            });
    }
}