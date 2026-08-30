using HotChocolate;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("ProductContent")]
public record GqlProductContent(
	[property: GraphQLIgnore]
	ProductContentDto ContentDto)
{
	[GraphQLName("quantity")]
	public int Quantity => ContentDto.Quantity;

	[GraphQLName("product")]
	public GqlProduct Product => new(ContentDto.Product);
}
