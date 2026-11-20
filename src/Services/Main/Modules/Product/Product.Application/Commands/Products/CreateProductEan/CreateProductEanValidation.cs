using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.Products.CreateProductEan;

public class CreateProductEanValidation : AbstractValidator<CreateProductEanCommand>
{
	public CreateProductEanValidation()
	{
		RuleFor(x => x.Ean)
			.Must(x => !string.IsNullOrWhiteSpace(x))
			.WithLocalizableError(ArticleEanRequiredMessage.Instance);

		RuleFor(x => x.Ean)
			.Must(x => x.Trim().Length <= 14)
			.When(x => !string.IsNullOrWhiteSpace(x.Ean))
			.WithLocalizableError(ArticleEanMaxLengthMessage.Instance);
	}
}
