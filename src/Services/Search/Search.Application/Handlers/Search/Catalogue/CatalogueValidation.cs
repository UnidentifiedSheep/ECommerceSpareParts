using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Search.Abstractions;
using Search.Enums;

namespace Search.Application.Handlers.Search.Catalogue;

public sealed class CatalogueValidation : AbstractValidator<SearchInCatalogueQuery>
{
	public CatalogueValidation()
	{
		RuleFor(x => x.Query)
			.MaximumLength(200)
			.WithLocalizableError(new CatalogueSearchQueryMaxLengthMessage().WithCount(200));

		RuleFor(x => x.Targets)
			.NotEmpty()
			.WithLocalizableError(CatalogueSearchTargetsRequiredMessage.Instance);
		RuleForEach(x => x.Targets)
			.IsInEnum()
			.WithLocalizableError(CatalogueSearchTargetInvalidMessage.Instance);

		RuleForEach(x => x.SkuModes)
			.IsInEnum()
			.WithLocalizableError(CatalogueSearchMatchModeInvalidMessage.Instance);
		RuleForEach(x => x.NameModes)
			.IsInEnum()
			.WithLocalizableError(CatalogueSearchMatchModeInvalidMessage.Instance);
		RuleFor(x => x.CandidateMappingStatus)
			.IsInEnum()
			.WithLocalizableError(CatalogueSearchCandidateMappingStatusInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => string.IsNullOrWhiteSpace(x.Query) || x.SkuModes.Count > 0 || x.NameModes.Count > 0)
			.WithLocalizableError(CatalogueSearchTextModeRequiredMessage.Instance);

		RuleFor(x => x)
			.Must(HasApplicableMode)
			.WithLocalizableError(new CatalogueSearchFuzzyQueryMinLengthMessage().WithCount(4));

		RuleFor(x => x.ProducerIds)
			.Must(ids => ids.Count <= 100)
			.WithLocalizableError(new CatalogueSearchProducerCountMaxMessage().WithCount(100));

		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}

	private static bool HasApplicableMode(SearchInCatalogueQuery query)
	{
		var length = query.Query?.Trim().Length ?? 0;
		if (length is 0 or >= 4)
			return true;

		return query.SkuModes.Any(mode => mode != SearchMatchType.Fuzzy) ||
			query.NameModes.Any(mode => mode != SearchMatchType.Fuzzy);
	}
}
