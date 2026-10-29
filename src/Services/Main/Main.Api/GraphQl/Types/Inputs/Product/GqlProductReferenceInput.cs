using Enums;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductReferenceInput")]
public sealed record GqlProductReferenceInput
{
	[GraphQLName("sku")]
	public required string Sku { get; init; }

	[GraphQLName("producerName")]
	public required string ProducerName { get; init; }

	[GraphQLName("supplier")]
	public Supplier? Supplier { get; init; }
}
