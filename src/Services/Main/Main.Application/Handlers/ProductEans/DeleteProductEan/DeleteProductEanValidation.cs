using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductEans.DeleteProductEan;

public class DeleteProductEanValidation : AbstractValidator<DeleteProductEanCommand>
{
	public DeleteProductEanValidation() =>
		RuleFor(x => x.Ean)
			.Must(x => !string.IsNullOrWhiteSpace(x))
			.WithLocalizableError(ArticleEanRequiredMessage.Instance);
}
