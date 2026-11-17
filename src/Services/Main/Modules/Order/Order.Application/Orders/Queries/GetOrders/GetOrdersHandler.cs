using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Querying;
using Main.Entities.Order.Enums.Orders;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Order.Application.Orders.Dtos;
using Main.Modules.Order.Application.Orders.Extensions;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;
using Security.Core.Interfaces;

namespace Main.Modules.Order.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
	RangeModel<DateTime> DateRange,
	Pagination Pagination,
	IReadOnlyCollection<Guid> OrganizationIds,
	IReadOnlyCollection<Guid> UserIds,
	IReadOnlyCollection<int> CurrencyIds,
	IReadOnlyCollection<int> ProductIds,
	IReadOnlyCollection<OrderStatus> Statuses,
	IReadOnlyCollection<OrderFulfillmentStatus> FulfillmentStatuses,
	IReadOnlyCollection<OrderSource> Sources,
	IReadOnlyCollection<SortExpression> SortBy,
	string? SearchTerm) : IQuery<GetOrdersResult>;

public sealed record GetOrdersResult(IReadOnlyList<OrderDto> Orders);

public sealed class GetOrdersHandler(
	IReadRepository<Entities.Order.Order.Order, Guid> repository,
	IOrganizationSearch organizationSearch,
	IUserSearch userSearch,
	IProductSearch productSearch,
	IProjectionProvider<Entities.Order.Order.Order, OrderDto> projection,
	IUserContext userContext) : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
	public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
	{
		var query = repository.Query.WhereReadableBy(userContext);

		if (request.DateRange.Min.HasValue)
			query = query.Where(order => order.CreatedAt >= request.DateRange.Min.Value);
		if (request.DateRange.Max.HasValue)
			query = query.Where(order => order.CreatedAt <= request.DateRange.Max.Value);
		if (request.OrganizationIds.Count > 0)
			query = query.Where(order => request.OrganizationIds.Contains(order.OrganizationId));
		if (request.UserIds.Count > 0)
			query =
				query.Where(order => order.UserId.HasValue && request.UserIds.Contains(order.UserId.Value));
		if (request.CurrencyIds.Count > 0)
			query = query.Where(order => request.CurrencyIds.Contains(order.CurrencyId));
		if (request.ProductIds.Count > 0)
			query = query.Where(order
				=> order.Items.Any(item => !item.IsDeleted && request.ProductIds.Contains(item.ProductId)));
		if (request.Statuses.Count > 0)
			query = query.Where(order => request.Statuses.Contains(order.Status));
		else
			query = query.Where(order => order.Status != OrderStatus.Deleted);
		if (request.FulfillmentStatuses.Count > 0)
			query = query.Where(order => request.FulfillmentStatuses.Contains(order.FulfillmentStatus));
		if (request.Sources.Count > 0)
			query = query.Where(order => request.Sources.Contains(order.Source));
		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
			query = ApplySearchTerm(query, request.SearchTerm);

		var orders = await query.SortBy(request.SortBy)
			.ThenBy(order => order.Id)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);
		return new GetOrdersResult(orders);
	}

	private IQueryable<Entities.Order.Order.Order> ApplySearchTerm(
		IQueryable<Entities.Order.Order.Order> query,
		string searchTerm)
	{
		var term = searchTerm.Trim();
		var isId = Guid.TryParse(term, out var id);
		var matchingOrganizations = organizationSearch.MatchingIds(term);
		var matchingUsers = userSearch.MatchingIds(term);
		var matchingProducts = productSearch.MatchingIds(term);

		return query.Where(order => isId && order.Id == id ||
			matchingOrganizations.Contains(order.OrganizationId) ||
			order.UserId.HasValue && matchingUsers.Contains(order.UserId.Value) ||
			order.Items.Any(item => !item.IsDeleted && matchingProducts.Contains(item.ProductId)));
	}
}
