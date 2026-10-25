using HotChocolate;
using HotChocolate.Types.Composite;

namespace Main.Api.GraphQl.Queries;

[GraphQLName("Query")]
public sealed class RootQuery
{
	[GraphQLName("users")]
	public UserQueries Users => new();

	[GraphQLName("organizations")]
	public OrganizationQueries Organizations => new();

	[GraphQLName("products")]
	public ProductQueries Product => new();

	[GraphQLName("productGroups")]
	public ProductGroupQueries ProductGroups => new();

	[GraphQLName("characteristics")]
	public CharacteristicQueries Characteristics => new();

	[GraphQLName("catalogueCandidates")]
	public CatalogueCandidateQueries CatalogueCandidate => new();

	[GraphQLName("producers")]
	[Shareable]
	public ProducerQueries Producer => new();

	[GraphQLName("storageContents")]
	public StorageContentQueries StorageContent => new();

	[GraphQLName("notifications")]
	public NotificationQueries Notification => new();

	[GraphQLName("documents")]
	public DocumentQueries Documents => new();
}
