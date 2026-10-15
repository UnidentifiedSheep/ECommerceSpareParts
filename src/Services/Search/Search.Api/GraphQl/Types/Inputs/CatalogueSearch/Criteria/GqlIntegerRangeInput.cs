using HotChocolate;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch.Criteria;

[GraphQLName("SearchIntegerRangeInput")]
public sealed record GqlIntegerRangeInput
{
	[GraphQLName("gt")]
	public int? Gt { get; init; }

	[GraphQLName("gte")]
	public int? Gte { get; init; }

	[GraphQLName("lt")]
	public int? Lt { get; init; }

	[GraphQLName("lte")]
	public int? Lte { get; init; }

	public RangeCriteria<int> ToCriteria() => new() { Gt = Gt, Gte = Gte, Lt = Lt, Lte = Lte };
}
