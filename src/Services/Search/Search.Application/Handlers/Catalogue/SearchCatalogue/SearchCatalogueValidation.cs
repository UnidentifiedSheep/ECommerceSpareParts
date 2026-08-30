using Application.Common.Validators;
using FluentValidation;
using Search.Enums;

namespace Search.Application.Handlers.Catalogue.SearchCatalogue;

public sealed class SearchCatalogueValidation : AbstractValidator<SearchCatalogueQuery>
{
	public SearchCatalogueValidation()
	{
		RuleFor(x => x.Query).MaximumLength(200);

		RuleFor(x => x.Targets).NotEmpty();
		RuleForEach(x => x.Targets).IsInEnum();

		RuleForEach(x => x.SkuModes).IsInEnum();
		RuleForEach(x => x.NameModes).IsInEnum();

		RuleFor(x => x)
			.Must(x => string.IsNullOrWhiteSpace(x.Query) || x.SkuModes.Count > 0 || x.NameModes.Count > 0)
			.WithMessage("At least one SKU or name search mode is required for a text query.");

		RuleFor(x => x)
			.Must(HasApplicableMode)
			.WithMessage("Fuzzy search requires a query of at least 4 characters.");

		RuleFor(x => x.ProducerIds)
			.Must(ids => ids.Count <= 100)
			.WithMessage("No more than 100 producers can be specified.");

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}

	private static bool HasApplicableMode(SearchCatalogueQuery query)
	{
		var length = query.Query?.Trim().Length ?? 0;
		if (length == 0 || length >= 4)
			return true;

		return query.SkuModes.Any(mode => mode != SearchMatchType.Fuzzy) ||
			query.NameModes.Any(mode => mode != SearchMatchType.Fuzzy);
	}
}
