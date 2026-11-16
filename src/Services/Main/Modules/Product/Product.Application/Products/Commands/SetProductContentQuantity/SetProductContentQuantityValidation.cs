using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Products.Commands.SetProductContentQuantity;

public class SetProductContentQuantityValidation : AbstractValidator<SetProductsContentCountCommand>
{
	public SetProductContentQuantityValidation()
	{
		RuleFor(x => x.Quantity)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(ArticleContentCountMustBeNonNegativeMessage.Instance);
	}
}
