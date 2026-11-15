using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductContentItemInput")]
public sealed record GqlProductContentItemInput
{
	[GraphQLName("childProductId")]
	public required int ChildProductId { get; init; }

	[GraphQLName("quantity")]
	public required int Quantity { get; init; }
}
