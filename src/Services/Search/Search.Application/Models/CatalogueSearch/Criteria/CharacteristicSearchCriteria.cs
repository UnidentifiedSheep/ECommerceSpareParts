namespace Search.Application.Models.CatalogueSearch.Criteria;

public sealed record CharacteristicSearchCriteria
{
	public required int CharacteristicId { get; init; }
	public IReadOnlyList<string>? Values { get; init; }
	public RangeCriteria<decimal>? NumericRange { get; init; }
	public bool? Exists { get; init; }
}
