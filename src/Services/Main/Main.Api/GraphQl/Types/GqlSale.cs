using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Sale;
using Main.Entities.Exceptions;
using Main.Enums;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Sale")]
public sealed record GqlSale
{
	private readonly SaleDto? _dto;

	public GqlSale(Guid id)
	{
		Id = id;
	}

	public GqlSale(SaleDto sale) : this(sale.Id)
	{
		_dto = sale;
	}

	[GraphQLName("id")]
	public Guid Id { get; }

	[GraphQLName("buyer")]
	public async Task<GqlUser> GetBuyerAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetSaleAsync(loader, cancellationToken)).Buyer);

	[GraphQLName("organization")]
	public async Task<GqlOrganization> GetOrganizationAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetSaleAsync(loader, cancellationToken)).Organization);

	[GraphQLName("comment")]
	public async Task<string?> GetCommentAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetSaleAsync(loader, cancellationToken)).Comment;

	[GraphQLName("saleDatetime")]
	public async Task<DateTime> GetSaleDatetimeAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetSaleAsync(loader, cancellationToken)).SaleDatetime;

	[GraphQLName("transaction")]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_GET_ALL)]
	public async Task<GqlTransaction> GetTransactionAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetSaleAsync(loader, cancellationToken)).TransactionId);

	[GraphQLName("totalSum")]
	public async Task<decimal> GetTotalSumAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetSaleAsync(loader, cancellationToken)).TotalSum;

	[GraphQLName("storage")]
	public async Task<GqlStorage> GetStorageAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetSaleAsync(loader, cancellationToken)).StorageCode);

	[GraphQLName("rowVersion")]
	public async Task<uint> GetRowVersionAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetSaleAsync(loader, cancellationToken)).RowVersion;

	[GraphQLName("state")]
	public async Task<SaleState> GetStateAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetSaleAsync(loader, cancellationToken)).State;

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetSaleAsync(loader, cancellationToken)).Currency);

	private async Task<SaleDto> GetSaleAsync(
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(Id, cancellationToken)
		?? throw new SaleNotFoundException(Id);
}
