using Application.Common.Extensions;
using FluentValidation;
using Search.Abstractions;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Application.Validators.CatalogueSearch;

public sealed class WeightSearchCriteriaValidator : AbstractValidator<WeightSearchCriteria>
{
	public WeightSearchCriteriaValidator()
	{
		RuleFor(criteria => criteria.Unit)
			.IsInEnum()
			.WithLocalizableError(CatalogueSearchCriteriaUnitInvalidMessage.Instance);

		RuleFor(criteria => criteria.Range)
			.NotNull()
			.WithLocalizableError(CatalogueSearchCriteriaRangeInvalidMessage.Instance)
			.SetValidator(new SearchRangeCriteriaValidator<decimal>());
	}
}
