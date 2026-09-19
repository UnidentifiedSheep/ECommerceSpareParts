using Application.Common.Extensions;
using FluentValidation;
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
