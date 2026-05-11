using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductSizes.SetProductSizes;

public class SetProductSizesValidation : AbstractValidator<SetProductSizesCommand>
{
    public SetProductSizesValidation()
    {
        RuleFor(x => x.Height)
            .GreaterThan(0)
            .WithLocalizationKey("article.size.height.must.be.greater.than.zero")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("article.size.height.max.two.decimals");

        RuleFor(x => x.Width)
            .GreaterThan(0)
            .WithLocalizationKey("article.size.width.must.be.greater.than.zero")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("article.size.width.max.two.decimals");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithLocalizationKey("article.size.length.must.be.greater.than.zero")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("article.size.length.max.two.decimals");
    }
}