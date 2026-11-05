using GraphQL.Common.Types;
using HotChocolate;
using Main.Enums.Orders;

namespace Main.Api.GraphQl.Types.Inputs.Order;

[GraphQLName("GetOrdersInput")]
public sealed record GqlGetOrdersInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("rangeStartDate")]
	public DateTime? RangeStartDate { get; init; }

	[GraphQLName("rangeEndDate")]
	public DateTime? RangeEndDate { get; init; }

	[GraphQLName("organizationIds")]
	public IReadOnlyCollection<Guid>? OrganizationIds { get; init; }

	[GraphQLName("userIds")]
	public IReadOnlyCollection<Guid>? UserIds { get; init; }

	[GraphQLName("currencyIds")]
	public IReadOnlyCollection<int>? CurrencyIds { get; init; }

	[GraphQLName("productIds")]
	public IReadOnlyCollection<int>? ProductIds { get; init; }

	[GraphQLName("statuses")]
	public IReadOnlyCollection<OrderStatus>? Statuses { get; init; }

	[GraphQLName("fulfillmentStatuses")]
	public IReadOnlyCollection<OrderFulfillmentStatus>? FulfillmentStatuses { get; init; }

	[GraphQLName("sources")]
	public IReadOnlyCollection<OrderSource>? Sources { get; init; }

	[GraphQLName("sortBy")]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }
}
