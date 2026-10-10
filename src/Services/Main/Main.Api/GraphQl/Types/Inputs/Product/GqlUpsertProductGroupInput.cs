using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("UpsertProductGroupInput")]
public sealed record GqlUpsertProductGroupInput
{
	[GraphQLName("id")]
	public int? Id { get; init; }

	[GraphQLName("name")]
	public required string Name { get; init; }
}
