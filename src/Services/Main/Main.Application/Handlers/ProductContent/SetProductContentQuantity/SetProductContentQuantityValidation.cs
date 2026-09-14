using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.ProductContent.SetProductContentQuantity;

public class SetProductContentQuantityValidation : AbstractValidator<SetProductsContentCountCommand>
{
	public SetProductContentQuantityValidation()
	{
		RuleFor(x => x.Quantity)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(ArticleContentCountMustBeNonNegativeMessage.Instance);
	}
}
