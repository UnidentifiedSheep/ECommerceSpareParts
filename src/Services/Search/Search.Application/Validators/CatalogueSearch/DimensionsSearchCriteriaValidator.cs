using Application.Common.Extensions;
using FluentValidation;
using Search.Abstractions;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Application.Validators.CatalogueSearch;

public sealed class DimensionsSearchCriteriaValidator : AbstractValidator<DimensionsSearchCriteria>
{
	public DimensionsSearchCriteriaValidator()
	{
		RuleFor(criteria => criteria.Unit).IsInEnum().WithLocalizableError(CatalogueSearchCriteriaUnitInvalidMessage.Instance);
		RuleFor(criteria => criteria).Must(criteria => criteria.Length is not null || criteria.Width is not null || criteria.Height is not null || criteria.Volume is not null)
			.WithLocalizableError(CatalogueSearchCriteriaDimensionsRequiredMessage.Instance);
		RuleFor(criteria => criteria.Length!).SetValidator(new SearchRangeCriteriaValidator<decimal>()).When(criteria => criteria.Length is not null);
		RuleFor(criteria => criteria.Width!).SetValidator(new SearchRangeCriteriaValidator<decimal>()).When(criteria => criteria.Width is not null);
		RuleFor(criteria => criteria.Height!).SetValidator(new SearchRangeCriteriaValidator<decimal>()).When(criteria => criteria.Height is not null);
		RuleFor(criteria => criteria.Volume!).SetValidator(new SearchRangeCriteriaValidator<decimal>()).When(criteria => criteria.Volume is not null);
	}
}
