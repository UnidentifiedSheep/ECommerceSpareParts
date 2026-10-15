using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("UpsertCharacteristicInput")]
public sealed record GqlUpsertCharacteristicInput
{
	[GraphQLName("id")]
	public int? Id { get; init; }

	[GraphQLName("name")]
	public required string Name { get; init; }
}
