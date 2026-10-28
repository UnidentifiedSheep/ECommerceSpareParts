using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("DeleteProductInput")]
public record GqlDeleteProductInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("rowVersion")]
	public required uint RowVersion { get; init; }
}
