using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductEanInput")]
public sealed record GqlProductEanInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("ean")]
	public required string Ean { get; init; }
}
