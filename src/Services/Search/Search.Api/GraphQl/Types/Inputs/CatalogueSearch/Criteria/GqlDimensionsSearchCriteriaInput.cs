using Enums.Units;
using HotChocolate;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch.Criteria;

[GraphQLName("ProductDimensionsCriteriaInput")]
public sealed record GqlDimensionsSearchCriteriaInput
{
	[GraphQLName("unit")]
	public required DimensionUnit Unit { get; init; }

	[GraphQLName("length")]
	public GqlNumericRangeInput? Length { get; init; }

	[GraphQLName("width")]
	public GqlNumericRangeInput? Width { get; init; }

	[GraphQLName("height")]
	public GqlNumericRangeInput? Height { get; init; }

	[GraphQLName("volume")]
	public GqlNumericRangeInput? Volume { get; init; }

	public DimensionsSearchCriteria ToCriteria() => new()
	{
		Unit = Unit, Length = Length, Width = Width, Height = Height, Volume = Volume
	};
}
