using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Orders;
using Main.Entities.Exceptions;
using Main.Enums.Orders;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Order")]
public sealed record GqlOrder
{
	private readonly OrderDto? _dto;

	public GqlOrder(Guid id)
	{
		Id = id;
	}

	public GqlOrder(OrderDto order) : this(order.Id)
	{
		_dto = order;
	}

	[GraphQLName("id")]
	public Guid Id { get; }

	[GraphQLName("organization")]
	public async Task<GqlOrganization> GetOrganizationAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetOrderAsync(loader, cancellationToken)).OrganizationId);

	[GraphQLName("user")]
	public async Task<GqlUser?> GetUserAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).UserId is { } id ? new GqlUser(id) : null;

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		IOrderByIdDataLoader loader,
		ICurrencyByIdDataLoader currencyLoader,
		CancellationToken cancellationToken)
	{
		var currencyId = (await GetOrderAsync(loader, cancellationToken)).CurrencyId;
		return new GqlCurrency(await currencyLoader.LoadAsync(currencyId, cancellationToken)
			?? throw new CurrencyNotFoundException(currencyId));
	}

	[GraphQLName("source")]
	public async Task<OrderSource> GetSourceAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).Source;

	[GraphQLName("status")]
	public async Task<OrderStatus> GetStatusAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).Status;

	[GraphQLName("fulfillmentStatus")]
	public async Task<OrderFulfillmentStatus> GetFulfillmentStatusAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).FulfillmentStatus;

	[GraphQLName("confirmedBy")]
	public async Task<GqlUser?> GetConfirmedByAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).ConfirmedByUserId is { } id ? new GqlUser(id) : null;

	[GraphQLName("confirmedAt")]
	public async Task<DateTime?> GetConfirmedAtAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).ConfirmedAt;

	[GraphQLName("createdAt")]
	public async Task<DateTime> GetCreatedAtAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).CreatedAt;

	[GraphQLName("updatedAt")]
	public async Task<DateTime> GetUpdatedAtAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrderAsync(loader, cancellationToken)).UpdatedAt;

	[GraphQLName("items")]
	public async Task<IReadOnlyList<GqlOrderItem>> GetItemsAsync(
		IOrderItemsByOrderIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(Id, cancellationToken) ?? [])
			.Select(item => new GqlOrderItem(item)).ToArray();

	private async Task<OrderDto> GetOrderAsync(
		IOrderByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(Id, cancellationToken)
		?? throw new OrderNotFoundException(Id);
}
