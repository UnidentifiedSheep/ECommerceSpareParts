using Enums.Units;
using HotChocolate;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch.Criteria;

[GraphQLName("ProductWeightCriteriaInput")]
public sealed record GqlWeightSearchCriteriaInput
{
	[GraphQLName("range")]
	public required GqlNumericRangeInput Range { get; init; }

	[GraphQLName("unit")]
	public required WeightUnit Unit { get; init; }

	public WeightSearchCriteria ToCriteria() => new() { Range = Range, Unit = Unit };
}
