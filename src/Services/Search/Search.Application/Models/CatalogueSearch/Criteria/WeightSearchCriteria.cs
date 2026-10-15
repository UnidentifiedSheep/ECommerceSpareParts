using Enums.Units;

namespace Search.Application.Models.CatalogueSearch.Criteria;

public sealed record WeightSearchCriteria
{
	public required RangeCriteria<decimal> Range { get; init; }
	public WeightUnit Unit { get; init; } = WeightUnit.Kilogram;
}
