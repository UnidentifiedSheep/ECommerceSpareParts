using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Application.Dtos.Purchase;
using Main.Enums;

namespace Main.Api.GraphQl.Types;

[GraphQLName("PurchaseLogistic")]
public sealed record GqlPurchaseLogistic(
	[property: GraphQLIgnore] PurchaseLogisticDto Logistics)
{
	[GraphQLName("routeId")]
	public Guid RouteId => Logistics.RouteId;

	[GraphQLName("pricingModel")]
	public LogisticPricingType PricingModel => Logistics.PricingModel;

	[GraphQLName("routeType")]
	public RouteType RouteType => Logistics.RouteType;

	[GraphQLName("priceKg")]
	public decimal PriceKg => Logistics.PriceKg;

	[GraphQLName("pricePerM3")]
	public decimal PricePerM3 => Logistics.PricePerM3;

	[GraphQLName("pricePerOrder")]
	public decimal PricePerOrder => Logistics.PricePerOrder;

	[GraphQLName("minimumPrice")]
	public decimal? MinimumPrice => Logistics.MinimumPrice;

	[GraphQLName("minimumPriceApplied")]
	public bool MinimumPriceApplied => Logistics.MinimumPriceApplied;

	[GraphQLName("currency")]
	public GqlCurrency Currency => new(Logistics.Currency);

	[GraphQLName("transaction")]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_GET_ALL)]
	public GqlTransaction? Transaction => Logistics.TransactionId is { } id ? new(id) : null;
}
