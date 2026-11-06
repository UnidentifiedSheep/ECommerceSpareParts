using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Orders.Dtos;
using Main.Application.Orders.Extensions;
using Main.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Security.Core.Interfaces;

namespace Main.Application.Orders.Queries;

public sealed record GetOrderItemsByOrderIdsQuery : IQuery<GetOrderItemsByOrderIdsResult>
{
	public GetOrderItemsByOrderIdsQuery(Guid id) : this([id]) { }

	public GetOrderItemsByOrderIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetOrderItemsByOrderIdsResult(IReadOnlyDictionary<Guid, IReadOnlyList<OrderItemDto>> Items);

public sealed class GetOrderItemsByOrderIdsHandler(
	IReadRepository<OrderItem, int> repository,
	IReadRepository<Order, Guid> orders,
	IProjectionProvider<OrderItem, OrderItemDto> projection,
	IUserContext userContext)
	: IQueryHandler<GetOrderItemsByOrderIdsQuery, GetOrderItemsByOrderIdsResult>
{
	public async Task<GetOrderItemsByOrderIdsResult> Handle(
		GetOrderItemsByOrderIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetOrderItemsByOrderIdsResult(new Dictionary<Guid, IReadOnlyList<OrderItemDto>>());

		var orderIds =  orders.Query.WhereReadableBy(userContext)
			.Where(order => request.Ids.Contains(order.Id))
			.Select(order => order.Id);

		var items = await repository.Query
			.Where(item => orderIds.Contains(item.OrderId))
			.OrderBy(item => item.Id)
			.Project(projection)
			.ToListAsync(cancellationToken);

		var byOrder = items.GroupBy(item => item.OrderId)
			.ToDictionary(group => group.Key, IReadOnlyList<OrderItemDto> (group) => group.ToArray());

		return new GetOrderItemsByOrderIdsResult(byOrder);
	}
}
