using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.DataLoaders;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.Types.Inputs.Order;
using MediatR;
using HotChocolate;
using Main.Api.GraphQl.Types.Order;
using Main.Application.Orders.Dtos;
using Main.Application.Orders.Queries.GetOrders;

namespace Main.Api.GraphQl.Queries;

public sealed class OrderQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAnyPermission(PermissionCodes.ORDERS_GET, PermissionCodes.ORDERS_GET_OWN)]
	public async Task<GqlOrder?> GetByIdAsync(
		Guid id,
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var order = await loader.LoadAsync(id, cancellationToken);
		return order is null ? null : new GqlOrder(order);
	}

	[GraphQLName("byIds")]
	[RequireAnyPermission(PermissionCodes.ORDERS_GET, PermissionCodes.ORDERS_GET_OWN)]
	public async Task<IReadOnlyList<GqlOrder>> GetByIdsAsync(
		IReadOnlyCollection<Guid> ids,
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(ids.Distinct().ToArray(), cancellationToken))
			.OfType<OrderDto>()
			.Select(order => new GqlOrder(order)).ToArray();

	[GraphQLName("list")]
	[RequireAnyPermission(PermissionCodes.ORDERS_GET, PermissionCodes.ORDERS_GET_OWN)]
	public async Task<IReadOnlyList<GqlOrder>> ListAsync(
		GqlGetOrdersInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetOrdersQuery(
			new RangeModel<DateTime>(input.RangeStartDate, input.RangeEndDate),
			input.Pagination,
			input.OrganizationIds ?? [],
			input.UserIds ?? [],
			input.CurrencyIds ?? [],
			input.ProductIds ?? [],
			input.Statuses ?? [],
			input.FulfillmentStatuses ?? [],
			input.Sources ?? [],
			input.SortBy.ToSortExpressions(),
			input.SearchTerm), cancellationToken);
		return result.Orders.Select(order => new GqlOrder(order)).ToArray();
	}
}
