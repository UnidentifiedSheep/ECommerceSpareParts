using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Products.MapImgsToProduct;

public class MapImgsToProductValidation : AbstractValidator<MapImgsToProductCommand>
{
    public static readonly string[] ImageExtensions =
    [
        ".png",
        ".jpeg",
        ".jpg",
        ".gif",
        ".bmp",
        ".webp",
        ".tiff"
    ];
    
    public MapImgsToProductValidation()
    {
        RuleForEach(x => x.Images)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Extension)
                    .Must(x => ImageExtensions.Any(c => c == x))
                    .WithLocalizationKey("article.image.invalid.extension");
            });

        RuleFor(x => x.Images)
            .NotEmpty()
            .WithLocalizationKey("article.images.must.not.be.empty");
    }
}