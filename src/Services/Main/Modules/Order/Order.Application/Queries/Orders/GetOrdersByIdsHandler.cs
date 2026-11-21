using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Modules.Order.Application.Orders.Dtos;
using Main.Modules.Order.Application.Orders.Extensions;
using Microsoft.EntityFrameworkCore;
using Security.Core.Interfaces;

namespace Main.Modules.Order.Application.Queries.Orders;

public sealed record GetOrdersByIdsQuery : IQuery<GetOrdersByIdsResult>
{
	public GetOrdersByIdsQuery(Guid id) : this([id]) { }

	public GetOrdersByIdsQuery(IEnumerable<Guid> ids) => Ids = ids.Distinct().ToArray();

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetOrdersByIdsResult(IReadOnlyDictionary<Guid, OrderDto> Orders);

public sealed class GetOrdersByIdsHandler(
	IReadRepository<Entities.Order.Order.Order, Guid> repository,
	IProjectionProvider<Entities.Order.Order.Order, OrderDto> projection,
	IUserContext userContext)
	: IQueryHandler<GetOrdersByIdsQuery, GetOrdersByIdsResult>
{
	public async Task<GetOrdersByIdsResult> Handle(
		GetOrdersByIdsQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query.WhereReadableBy(userContext);

		if (request.Ids.Count == 0)
			return new GetOrdersByIdsResult(new Dictionary<Guid, OrderDto>());

		var orders = await query
			.Where(order => request.Ids.Contains(order.Id))
			.Project(projection)
			.ToDictionaryAsync(order => order.Id, cancellationToken);

		return new GetOrdersByIdsResult(orders);
	}
}
