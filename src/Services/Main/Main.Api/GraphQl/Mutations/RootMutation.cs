using HotChocolate;

namespace Main.Api.GraphQl.Mutations;

public sealed class RootMutation
{
	[GraphQLName("catalogueCandidates")]
	public CatalogueCandidateMutations CatalogueCandidate => new();
}
