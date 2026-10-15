using Application.Common.Extensions;
using FluentValidation;
using Search.Abstractions;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Application.Validators.CatalogueSearch;

public sealed class SearchRangeCriteriaValidator<T> : AbstractValidator<RangeCriteria<T>> where T : struct, IComparable<T>
{
	public SearchRangeCriteriaValidator()
	{
		RuleFor(range => range).Must(IsValidRange)
			.WithLocalizableError(CatalogueSearchCriteriaRangeInvalidMessage.Instance);
	}

	private static bool IsValidRange(RangeCriteria<T> range)
	{
		if (range.Gt.HasValue && range.Gte.HasValue ||
			range.Lt.HasValue && range.Lte.HasValue)
			return false;

		var lower = range.Gt ?? range.Gte;
		var upper = range.Lt ?? range.Lte;
		if (!lower.HasValue && !upper.HasValue) return false;
		if (!lower.HasValue || !upper.HasValue) return true;
		var comparison = lower.Value.CompareTo(upper.Value);
		return comparison < 0 || comparison == 0 && !range.Gt.HasValue && !range.Lt.HasValue;
	}
}
