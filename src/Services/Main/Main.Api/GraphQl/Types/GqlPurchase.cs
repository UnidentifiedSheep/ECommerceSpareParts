using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Purchase;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Purchase")]
public sealed record GqlPurchase
{
	private readonly PurchaseDto? _dto;

	public GqlPurchase(Guid id)
	{
		Id = id;
	}

	public GqlPurchase(PurchaseDto purchase) : this(purchase.Id)
	{
		_dto = purchase;
	}

	[GraphQLName("id")]
	public Guid Id { get; }

	[GraphQLName("supplier")]
	public async Task<GqlUser> GetSupplierAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetPurchaseAsync(loader, cancellationToken)).Supplier);

	[GraphQLName("supplierOrganization")]
	public async Task<GqlOrganization> GetSupplierOrganizationAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetPurchaseAsync(loader, cancellationToken)).SupplierOrganization);

	[GraphQLName("comment")]
	public async Task<string?> GetCommentAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetPurchaseAsync(loader, cancellationToken)).Comment;

	[GraphQLName("purchaseDatetime")]
	public async Task<DateTime> GetPurchaseDatetimeAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetPurchaseAsync(loader, cancellationToken)).PurchaseDatetime;

	[GraphQLName("transaction")]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_GET_ALL)]
	public async Task<GqlTransaction> GetTransactionAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetPurchaseAsync(loader, cancellationToken)).TransactionId);

	[GraphQLName("totalSum")]
	public async Task<decimal> GetTotalSumAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetPurchaseAsync(loader, cancellationToken)).TotalSum;

	[GraphQLName("storage")]
	public async Task<GqlStorage> GetStorageAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetPurchaseAsync(loader, cancellationToken)).StorageCode);

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetPurchaseAsync(loader, cancellationToken)).Currency);

	[GraphQLName("contents")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_GET)]
	public async Task<IReadOnlyList<GqlPurchaseContent>> GetContentsAsync(
		IPurchaseContentsByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(Id, cancellationToken) ?? [])
			.Select(content => new GqlPurchaseContent(content)).ToArray();

	[GraphQLName("logistics")]
	public async Task<GqlPurchaseLogistic?> GetLogisticsAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var logistics = (await GetPurchaseAsync(loader, cancellationToken)).Logistics;
		return logistics is null ? null : new GqlPurchaseLogistic(logistics);
	}

	private async Task<PurchaseDto> GetPurchaseAsync(
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(Id, cancellationToken)
		?? throw new PurchaseNotFoundException(Id);
}
