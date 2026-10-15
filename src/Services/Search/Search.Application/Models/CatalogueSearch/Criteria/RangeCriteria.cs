namespace Search.Application.Models.CatalogueSearch.Criteria;

public sealed record RangeCriteria<T> where T : struct, IComparable<T>
{
	public T? Gt { get; init; }
	public T? Gte { get; init; }
	public T? Lt { get; init; }
	public T? Lte { get; init; }
}
