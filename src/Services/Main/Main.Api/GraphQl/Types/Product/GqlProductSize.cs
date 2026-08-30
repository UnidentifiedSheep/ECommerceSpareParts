using Enums.Units;
using HotChocolate;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("ProductSize")]
public record GqlProductSize(
	[property: GraphQLIgnore]
	ProductSizeDto ProductSize)
{
	[GraphQLName("length")]
	public decimal Length => ProductSize.Length;

	[GraphQLName("width")]
	public decimal Width => ProductSize.Width;

	[GraphQLName("height")]
	public decimal Height => ProductSize.Height;

	[GraphQLName("unit")]
	public DimensionUnit Unit => ProductSize.Unit;

	[GraphQLName("volumeM3")]
	public decimal VolumeM3 => ProductSize.VolumeM3;
}
