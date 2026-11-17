using HotChocolate;
using Pricing.Api.GraphQl.Mutations;

namespace Main.Api.GraphQl.Mutations;

[GraphQLName("Mutation")]
public sealed class RootMutation
{
	[GraphQLName("orders")]
	public OrderMutations Orders => new();

	[GraphQLName("users")]
	public UserMutations Users => new();

	[GraphQLName("roles")]
	public RoleMutations Roles => new();

	[GraphQLName("currencies")]
	public CurrencyMutations Currencies => new();

	[GraphQLName("producers")]
	public ProducerMutations Producers => new();

	[GraphQLName("storages")]
	public StorageMutations Storages => new();

	[GraphQLName("uploads")]
	public UploadMutations Uploads => new();

	[GraphQLName("storageContents")]
	public StorageContentMutations StorageContents => new();

	[GraphQLName("organizations")]
	public OrganizationMutations Organizations => new();

	[GraphQLName("transactions")]
	public TransactionMutations Transactions => new();

	[GraphQLName("purchases")]
	public PurchaseMutations Purchases => new();

	[GraphQLName("sales")]
	public SaleMutations Sales => new();

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

	[GraphQLName("markupGroups")]
	public MarkupGroupMutations MarkupGroups => new();

	[GraphQLName("priceAppliers")]
	public PriceApplierMutations PriceAppliers => new();
}
