using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Product;

namespace Main.Application.Products.Commands.SetProductContentQuantity;

public class SetProductContentQuantityValidation : AbstractValidator<SetProductsContentCountCommand>
{
	public SetProductContentQuantityValidation()
	{
		RuleFor(x => x.Quantity)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(ArticleContentCountMustBeNonNegativeMessage.Instance);
	}
}
