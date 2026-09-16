using HotChocolate;

namespace Main.Api.GraphQl.Mutations;

[GraphQLName("Mutation")]
public sealed class RootMutation
{
	[GraphQLName("catalogueCandidates")]
	public CatalogueCandidateMutations CatalogueCandidates => new();
}
