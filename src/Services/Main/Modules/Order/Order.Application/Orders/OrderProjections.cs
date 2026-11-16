using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Entities.Order.Order;
using Main.Modules.Order.Application.Orders.Dtos;

namespace Main.Modules.Order.Application.Orders;

[Lifetime(Lifetime.Singleton)]
public sealed class OrderDtoProjectionProvider : ProjectionProviderBase<Entities.Order.Order.Order, OrderDto>
{
	public override Expression<Func<Entities.Order.Order.Order, OrderDto>> Projection { get; } = order
		=> new OrderDto
		{
			Id = order.Id,
			OrganizationId = order.OrganizationId,
			UserId = order.UserId,
			CurrencyId = order.CurrencyId,
			Source = order.Source,
			Status = order.Status,
			RowVersion = order.RowVersion,
			FulfillmentStatus = order.FulfillmentStatus,
			ConfirmedByUserId = order.ConfirmedByUserId,
			ConfirmedAt = order.ConfirmedAt,
			CreatedAt = order.CreatedAt,
			UpdatedAt = order.UpdatedAt
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class OrderItemDtoProjectionProvider : ProjectionProviderBase<OrderItem, OrderItemDto>
{
	public override Expression<Func<OrderItem, OrderItemDto>> Projection { get; } = item => new OrderItemDto
	{
		Id = item.Id,
		OrderId = item.OrderId,
		ProductId = item.ProductId,
		Count = item.Count,
		UnitPrice = item.UnitPrice,
		PriceOrigin = item.PriceOrigin
	};
}
