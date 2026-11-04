using Application.Common;
using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public class GetProductCrossesValidation : AbstractValidator<GetProductCrossesQuery>
{
	public GetProductCrossesValidation()
	{
		RuleFor(query => query.Pagination).SetValidator(new PaginationValidator());
		RuleFor(query => query.SearchTerm)
			.MaximumLength(200)
			.WithLocalizableError(SearchStringMaxLengthMessage.Create(200));
		RuleFor(query => query.StockStorageCode)
			.Must(code => code == null || code.Trim().Length <= 128)
			.WithLocalizableError(StorageCodeMaxLengthMessage.Instance);
	}
}
