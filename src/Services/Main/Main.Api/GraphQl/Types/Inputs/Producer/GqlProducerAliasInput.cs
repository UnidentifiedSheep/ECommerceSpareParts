using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Producer;

[GraphQLName("ProducerAliasInput")]
public sealed record GqlProducerAliasInput
{
	[GraphQLName("producerId")]
	public required int ProducerId { get; init; }

	[GraphQLName("alias")]
	public required string Alias { get; init; }
}
