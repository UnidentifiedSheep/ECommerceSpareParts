using HotChocolate;

namespace Main.Api.GraphQl.Mutations;

[GraphQLName("Mutation")]
public sealed class RootMutation
{
	[GraphQLName("users")]
	public UserMutations Users => new();

	[GraphQLName("catalogueCandidates")]
	public CatalogueCandidateMutations CatalogueCandidates => new();

	[GraphQLName("notifications")]
	public NotificationMutations Notifications => new();

	[GraphQLName("documents")]
	public DocumentMutations Documents => new();

	[GraphQLName("productGroups")]
	public ProductGroupMutations ProductGroups => new();

	[GraphQLName("characteristics")]
	public CharacteristicMutations Characteristics => new();

	[GraphQLName("products")]
	public ProductMutations Products => new();

	[GraphQLName("productEans")]
	public ProductEanMutation ProductEans => new();
}
