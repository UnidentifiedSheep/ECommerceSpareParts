using Enums.Units;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("SetProductSizeInput")]
public sealed record GqlSetProductSizeInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("length")]
	public required decimal Length { get; init; }

	[GraphQLName("width")]
	public required decimal Width { get; init; }

	[GraphQLName("height")]
	public required decimal Height { get; init; }

	[GraphQLName("unit")]
	public required DimensionUnit Unit { get; init; }
}
