using HotChocolate;
using Main.Application.Dtos.Purchase;

namespace Main.Api.GraphQl.Types;

[GraphQLName("PurchaseContentLogistic")]
public sealed record GqlPurchaseContentLogistic(
	[property: GraphQLIgnore] PurchaseContentLogisticDto Logistics)
{
	[GraphQLName("weightKg")]
	public decimal WeightKg => Logistics.WeightKg;

	[GraphQLName("areaM3")]
	public decimal AreaM3 => Logistics.AreaM3;

	[GraphQLName("price")]
	public decimal Price => Logistics.Price;
}
