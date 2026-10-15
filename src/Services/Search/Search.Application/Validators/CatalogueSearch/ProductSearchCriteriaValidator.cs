using Application.Common.Extensions;
using FluentValidation;
using Search.Abstractions;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Application.Validators.CatalogueSearch;

public sealed class ProductSearchCriteriaValidator : AbstractValidator<ProductSearchCriteria>
{
	public ProductSearchCriteriaValidator()
	{
		RuleFor(criteria => criteria.Stock!)
			.SetValidator(new SearchRangeCriteriaValidator<int>())
			.When(criteria => criteria.Stock is not null);

		RuleFor(criteria => criteria.Weight!)
			.SetValidator(new WeightSearchCriteriaValidator())
			.When(criteria => criteria.Weight is not null);

		RuleFor(criteria => criteria.Dimensions!)
			.SetValidator(new DimensionsSearchCriteriaValidator())
			.When(criteria => criteria.Dimensions is not null);

		RuleFor(criteria => criteria.Characteristics)
			.Must(criteria => criteria.Count <= 100)
			.WithLocalizableError(new CatalogueSearchCriteriaCharacteristicCountMaxMessage().WithCount(100));

		RuleFor(criteria => criteria.Characteristics)
			.Must(criteria => criteria
				.Select(item => item.CharacteristicId)
				.Distinct()
				.Count() == criteria.Count)
			.WithLocalizableError(CatalogueSearchCriteriaCharacteristicDuplicateMessage.Instance);

		RuleForEach(criteria => criteria.Characteristics)
			.SetValidator(new CharacteristicSearchCriteriaValidator());
	}
}
