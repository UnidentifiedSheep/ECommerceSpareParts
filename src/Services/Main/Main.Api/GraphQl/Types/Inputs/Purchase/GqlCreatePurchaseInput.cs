using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Purchase;

[GraphQLName("CreatePurchaseInput")]
public sealed record GqlCreatePurchaseInput
{
	[GraphQLName("supplierUserId")]
	public required Guid SupplierUserId { get; init; }

	[GraphQLName("supplierOrganizationId")]
	public required Guid SupplierOrganizationId { get; init; }

	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }

	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("purchaseDate")]
	public required DateTime PurchaseDate { get; init; }

	[GraphQLName("contents")]
	public required IReadOnlyList<GqlNewPurchaseContentInput> Contents { get; init; }

	[GraphQLName("comment")]
	public string? Comment { get; init; }

	[GraphQLName("payedSum")]
	public decimal? PayedSum { get; init; }

	[GraphQLName("withLogistics")]
	public bool? WithLogistics { get; init; }

	[GraphQLName("storageFrom")]
	public string? StorageFrom { get; init; }

	[GraphQLName("forcePayment")]
	public bool? ForcePayment { get; init; }
}
