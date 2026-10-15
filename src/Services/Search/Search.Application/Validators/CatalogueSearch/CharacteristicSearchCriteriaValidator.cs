using Application.Common.Extensions;
using FluentValidation;
using Search.Abstractions;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Application.Validators.CatalogueSearch;

public sealed class CharacteristicSearchCriteriaValidator : AbstractValidator<CharacteristicSearchCriteria>
{
	public CharacteristicSearchCriteriaValidator()
	{
		RuleFor(criteria => criteria.CharacteristicId)
			.GreaterThan(0)
			.WithLocalizableError(CatalogueSearchCriteriaCharacteristicIdInvalidMessage.Instance);

		RuleFor(criteria => criteria)
			.Must(criteria => (criteria.Values is null ? 0 : 1) + (criteria.NumericRange is null ? 0 : 1) + (criteria.Exists.HasValue ? 1 : 0) == 1)
			.WithLocalizableError(CatalogueSearchCriteriaCharacteristicOperationRequiredMessage.Instance);

		RuleFor(criteria => criteria.NumericRange!)
			.SetValidator(new SearchRangeCriteriaValidator<decimal>())
			.When(criteria => criteria.NumericRange is not null);

		When(criteria => criteria.Values is not null, () =>
		{
			RuleFor(criteria => criteria.Values!).Must(values => values.Count is > 0 and <= 100)
				.WithLocalizableError(new CatalogueSearchCriteriaValuesCountInvalidMessage()
					.WithCount(100));
			RuleForEach(criteria => criteria.Values!).NotEmpty()
				.WithLocalizableError(CatalogueSearchCriteriaValueRequiredMessage.Instance)
				.MaximumLength(1024)
				.WithLocalizableError(new CatalogueSearchCriteriaValueMaxLengthMessage()
					.WithCount(1024));
		});
	}
}
