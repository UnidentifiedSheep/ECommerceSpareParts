using System.Diagnostics.CodeAnalysis;
using HotChocolate;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch.Criteria;

[GraphQLName("SearchNumericRangeInput")]
public sealed record GqlNumericRangeInput
{
	[GraphQLName("gt")]
	public decimal? Gt { get; init; }

	[GraphQLName("gte")]
	public decimal? Gte { get; init; }

	[GraphQLName("lt")]
	public decimal? Lt { get; init; }

	[GraphQLName("lte")]
	public decimal? Lte { get; init; }

	[return: NotNullIfNotNull(nameof(input))]
	public static implicit operator RangeCriteria<decimal>?(GqlNumericRangeInput? input) =>
		input is null ? null : new RangeCriteria<decimal>
		{
			Gt = input.Gt,
			Gte = input.Gte,
			Lt = input.Lt,
			Lte = input.Lte
		};

}
