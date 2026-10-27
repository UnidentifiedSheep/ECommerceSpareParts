using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Purchase;
using Main.Application.Handlers.Purchases.GetPurchase;
using Main.Application.Handlers.Purchases.GetPurchases;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class PurchaseQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.PURCHASE_GET)]
	public async Task<GqlPurchase?> GetByIdAsync(
		Guid id,
		IPurchaseByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var purchase = await loader.LoadAsync(id, cancellationToken);
		return purchase is null ? null : new GqlPurchase(purchase);
	}

	[GraphQLName("byTransactionId")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_GET)]
	public async Task<GqlPurchase> GetByTransactionIdAsync(
		Guid transactionId,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetPurchaseQuery(null, transactionId), cancellationToken);
		return new GqlPurchase(result.Purchase);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_GET)]
	public async Task<IReadOnlyList<GqlPurchase>> ListAsync(
		GqlGetPurchasesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetPurchasesQuery(
				new RangeModel<DateTime>(input.RangeStartDate, input.RangeEndDate),
				input.Pagination,
				input.SupplierOrganizationIds ?? [],
				input.CurrencyIds ?? [],
				input.ProductIds ?? [],
				input.SortBy.ToSortExpressions(),
				input.SearchTerm),
			cancellationToken);

		return result.Purchases.Select(purchase => new GqlPurchase(purchase)).ToArray();
	}
}
