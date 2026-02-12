using Constants;
using FluentValidation;

namespace Main.Application.Handlers.ArticleImages.MapImgsToArticle;

public class MapImgsToArticleValidation : AbstractValidator<MapImgsToArticleCommand>
{
    public MapImgsToArticleValidation()
    {
        RuleForEach(x => x.Images)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Extension)
                    .Must(x => FileConstants.ImageExtensions.Any(c => c == x))
                    .WithMessage("Файл должен являться изображением");
            });

        RuleFor(x => x.Images)
            .NotEmpty()
            .WithMessage("Список изображений не может быть пуст");
    }
}