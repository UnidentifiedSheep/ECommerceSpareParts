using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.Types;

namespace Main.Api.GraphQl.Subscriptions;

[SubscriptionType]
public static partial class SaleSubscription
{
	[EventStream("sale { id }")]
	[GraphQLName("onSaleCreated")]
	[RequireAllPermissions(PermissionCodes.SALES_GET)]
	public static SaleCreated OnSaleCreated()
		=> EventStream.Create<SaleCreated>();

	[EventStream("sale { id }")]
	[GraphQLName("onSaleUpdated")]
	[RequireAllPermissions(PermissionCodes.SALES_GET)]
	public static SaleUpdated OnSaleUpdated()
		=> EventStream.Create<SaleUpdated>();

	[EventStream("sale { id }")]
	[GraphQLName("onSaleDeleted")]
	[RequireAllPermissions(PermissionCodes.SALES_GET)]
	public static SaleDeleted OnSaleDeleted()
		=> EventStream.Create<SaleDeleted>();
}

public record SaleCreated(
	[property: GraphQLName("sale")] GqlSale Sale);

public record SaleUpdated(
	[property: GraphQLName("sale")] GqlSale Sale);

public record SaleDeleted(
	[property: GraphQLName("sale")] GqlSale Sale);
