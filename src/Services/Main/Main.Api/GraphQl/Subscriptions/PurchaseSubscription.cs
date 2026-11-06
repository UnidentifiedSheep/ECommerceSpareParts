using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Purchase;

namespace Main.Api.GraphQl.Subscriptions;

[SubscriptionType]
public static partial class PurchaseSubscription
{
	[EventStream("purchase { id }")]
	[GraphQLName("onPurchaseCreated")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_GET)]
	public static PurchaseCreated OnPurchaseCreated()
		=> EventStream.Create<PurchaseCreated>();

	[EventStream("purchase { id }")]
	[GraphQLName("onPurchaseUpdated")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_GET)]
	public static PurchaseUpdated OnPurchaseUpdated()
		=> EventStream.Create<PurchaseUpdated>();

	[EventStream("purchaseId")]
	[GraphQLName("onPurchaseDeleted")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_GET)]
	public static PurchaseDeleted OnPurchaseDeleted()
		=> EventStream.Create<PurchaseDeleted>();
}

public record PurchaseCreated(
	[property: GraphQLName("purchase")] GqlPurchase Purchase);

public record PurchaseUpdated(
	[property: GraphQLName("purchase")] GqlPurchase Purchase);

public record PurchaseDeleted(
	[property: GraphQLName("purchaseId")] Guid PurchaseId);
