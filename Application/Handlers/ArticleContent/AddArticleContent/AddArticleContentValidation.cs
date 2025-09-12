using FluentValidation;

namespace Application.Handlers.ArticleContent.AddArticleContent;

public class AddArticleContentValidation : AbstractValidator<AddArticleContentCommand>
{
    public AddArticleContentValidation()
    {
        RuleForEach(cmd => cmd.Content)
            .Must((parent, kvp) => kvp.Key != parent.ArticleId)
            .WithMessage("Артикул не может быть содержимым самого себя.");

        RuleForEach(cmd => cmd.Content)
            .Must(kvp => kvp.Value >= 0)
            .WithMessage("Количество должно быть больше или равно 0.");
    }
}