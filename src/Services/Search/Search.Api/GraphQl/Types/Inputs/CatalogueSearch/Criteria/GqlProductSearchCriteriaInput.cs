using HotChocolate;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch.Criteria;

[GraphQLName("ProductSearchCriteriaInput")]
public sealed record GqlProductSearchCriteriaInput
{
	[GraphQLName("weight")]
	public GqlWeightSearchCriteriaInput? Weight { get; init; }

	[GraphQLName("dimensions")]
	public GqlDimensionsSearchCriteriaInput? Dimensions { get; init; }

	[GraphQLName("characteristics")]
	public IReadOnlyList<GqlCharacteristicSearchCriteriaInput>? Characteristics { get; init; }

	public ProductSearchCriteria ToCriteria() => new()
	{
		Weight = Weight?.ToCriteria(),
		Dimensions = Dimensions?.ToCriteria(),
		Characteristics = Characteristics?.Select(characteristic => characteristic.ToCriteria()).ToArray() ?? []
	};
}
