using HotChocolate;
using HotChocolate.Types.Composite;

namespace Main.Api.GraphQl.Queries.Root;

public sealed class Query
{
	[GraphQLName("products")]
	public ProductQueries Product => new();

	[GraphQLName("catalogueCandidates")]
	public CatalogueCandidateQueries CatalogueCandidate => new();

	[GraphQLName("producers")]
	[Shareable]
	public ProducerQueries Producer => new();

	[GraphQLName("storageContents")]
	public StorageContentQueries StorageContent => new();
}
