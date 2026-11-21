using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.Products.DeleteProductEan;

public class DeleteProductEanValidation : AbstractValidator<DeleteProductEanCommand>
{
	public DeleteProductEanValidation()
		=> RuleFor(x => x.Ean)
			.Must(x => !string.IsNullOrWhiteSpace(x))
			.WithLocalizableError(ArticleEanRequiredMessage.Instance);
}
