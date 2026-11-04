using GreenDonut;
using Main.Application.Dtos.Orders;
using Main.Application.Handlers.Orders;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class OrderDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, OrderDto>> GetOrderByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken) =>
		(await sender.Send(new GetOrdersByIdsQuery(keys), cancellationToken)).Orders;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<OrderItemDto>>> GetOrderItemsByOrderIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken) =>
		(await sender.Send(new GetOrderItemsByOrderIdsQuery(keys), cancellationToken)).Items;
}
