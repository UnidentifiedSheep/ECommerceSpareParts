using HotChocolate;
using HotChocolate.Types.Composite;

namespace Main.Api.GraphQl.Queries;

[GraphQLName("Query")]
public sealed class RootQuery
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

	[GraphQLName("notifications")]
	public NotificationQueries Notification => new();
}
