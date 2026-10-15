using Enums.Units;

namespace Search.Application.Models.CatalogueSearch.Criteria;

public sealed record DimensionsSearchCriteria
{
	public DimensionUnit Unit { get; init; } = DimensionUnit.Meter;
	public RangeCriteria<decimal>? Length { get; init; }
	public RangeCriteria<decimal>? Width { get; init; }
	public RangeCriteria<decimal>? Height { get; init; }
	public RangeCriteria<decimal>? Volume { get; init; }
}
