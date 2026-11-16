using HotChocolate;
using Main.Api.GraphQl.Types.Product;
using Main.Entities.Order.Enums.Orders;
using Main.Modules.Order.Application.Orders.Dtos;

namespace Main.Api.GraphQl.Types.Order;

[GraphQLName("OrderItem")]
public sealed record GqlOrderItem([property: GraphQLIgnore]
	OrderItemDto Dto)
{
	[GraphQLName("id")]
	public int Id => Dto.Id;

	[GraphQLName("product")]
	public GqlProduct Product => new(Dto.ProductId);

	[GraphQLName("count")]
	public int Count => Dto.Count;

	[GraphQLName("unitPrice")]
	public decimal? UnitPrice => Dto.UnitPrice;

	[GraphQLName("totalPrice")]
	public decimal? TotalPrice => Dto.TotalPrice;

	[GraphQLName("priceOrigin")]
	public PriceOrigin PriceOrigin => Dto.PriceOrigin;
}
