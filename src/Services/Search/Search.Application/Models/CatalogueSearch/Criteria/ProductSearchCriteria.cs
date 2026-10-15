namespace Search.Application.Models.CatalogueSearch.Criteria;

public sealed record ProductSearchCriteria
{
	public RangeCriteria<int>? Stock { get; init; }
	public WeightSearchCriteria? Weight { get; init; }
	public DimensionsSearchCriteria? Dimensions { get; init; }
	public IReadOnlyList<CharacteristicSearchCriteria> Characteristics { get; init; } = [];
}
