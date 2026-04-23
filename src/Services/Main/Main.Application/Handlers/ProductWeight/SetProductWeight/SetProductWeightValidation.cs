using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductWeight.SetProductWeight;

public class SetProductWeightValidation : AbstractValidator<SetArticleWeightCommand>
{
    public SetProductWeightValidation()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithLocalizationKey("article.weight.must.be.greater.than.zero")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("article.weight.max.two.decimals");
    }
}