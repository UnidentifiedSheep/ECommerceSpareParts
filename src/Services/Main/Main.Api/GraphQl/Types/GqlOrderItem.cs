using HotChocolate;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Orders;
using Main.Enums.Orders;

namespace Main.Api.GraphQl.Types;

[GraphQLName("OrderItem")]
public sealed record GqlOrderItem([property: GraphQLIgnore] OrderItemDto Dto)
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
