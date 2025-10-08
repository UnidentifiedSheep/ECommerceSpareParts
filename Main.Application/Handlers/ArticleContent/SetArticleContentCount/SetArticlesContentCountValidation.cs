using FluentValidation;

namespace Main.Application.Handlers.ArticleContent.SetArticleContentCount;

public class SetArticlesContentCountValidation : AbstractValidator<SetArticlesContentCountCommand>
{
    public SetArticlesContentCountValidation()
    {
        RuleFor(x => x.Count).GreaterThanOrEqualTo(0)
            .WithMessage("Количество должно быть больше или равно 0");
    }
}