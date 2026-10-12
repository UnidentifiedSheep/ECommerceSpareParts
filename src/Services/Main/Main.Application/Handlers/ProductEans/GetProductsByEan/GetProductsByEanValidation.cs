using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductEans.GetProductsByEan;

public class GetProductsByEanValidation : AbstractValidator<GetProductsByEanQuery>
{
	public GetProductsByEanValidation()
	{
		RuleFor(x => x.Ean)
			.Must(x => !string.IsNullOrWhiteSpace(x))
			.WithLocalizableError(ArticleEanRequiredMessage.Instance);

		RuleFor(x => x.Ean)
			.Must(x => x.Trim().Length <= 14)
			.When(x => !string.IsNullOrWhiteSpace(x.Ean))
			.WithLocalizableError(ArticleEanMaxLengthMessage.Instance);

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
