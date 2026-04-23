using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductContent.SetProductContentQuantity;

public class SetProductContentQuantityValidation : AbstractValidator<SetProductsContentCountCommand>
{
    public SetProductContentQuantityValidation()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("article.content.count.must.be.non.negative");
    }
}