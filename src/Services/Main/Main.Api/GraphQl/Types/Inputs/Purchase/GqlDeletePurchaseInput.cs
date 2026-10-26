using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Purchase;

[GraphQLName("DeletePurchaseInput")]
public sealed record GqlDeletePurchaseInput
{
	[GraphQLName("purchaseId")]
	public required Guid PurchaseId { get; init; }
}
