using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Purchase;

[GraphQLName("UpdatePurchaseInput")]
public sealed record GqlUpdatePurchaseInput
{
	[GraphQLName("purchaseId")]
	public required Guid PurchaseId { get; init; }

	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("purchaseDateTime")]
	public required DateTime PurchaseDateTime { get; init; }

	[GraphQLName("contents")]
	public required IReadOnlyList<GqlEditPurchaseContentInput> Contents { get; init; }

	[GraphQLName("comment")]
	public string? Comment { get; init; }

	[GraphQLName("withLogistics")]
	public bool? WithLogistics { get; init; }

	[GraphQLName("storageFrom")]
	public string? StorageFrom { get; init; }
}
