using Constants;
using FluentValidation;
using Localization.Domain.Extensions;

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
                    .WithLocalizationKey("article.image.invalid.extension");
            });

        RuleFor(x => x.Images)
            .NotEmpty()
            .WithLocalizationKey("article.images.must.not.be.empty");
    }
}