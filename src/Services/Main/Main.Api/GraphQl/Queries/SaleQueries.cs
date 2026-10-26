using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Sale;
using Main.Application.Handlers.Sales.GetSale;
using Main.Application.Handlers.Sales.GetSales;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class SaleQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.SALES_GET)]
	public async Task<GqlSale?> GetByIdAsync(
		Guid id,
		ISaleByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var sale = await loader.LoadAsync(id, cancellationToken);
		return sale is null ? null : new GqlSale(sale);
	}

	[GraphQLName("byTransactionId")]
	[RequireAllPermissions(PermissionCodes.SALES_GET)]
	public async Task<GqlSale> GetByTransactionIdAsync(
		Guid transactionId,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetSaleQuery(null, transactionId), cancellationToken);
		return new GqlSale(result.Sale);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.SALES_GET)]
	public async Task<IReadOnlyList<GqlSale>> ListAsync(
		GqlGetSalesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetSalesQuery(
				new RangeModel<DateTime>(input.RangeStartDate, input.RangeEndDate),
				input.Pagination,
				input.OrganizationIds ?? [],
				input.CurrencyIds ?? [],
				input.ProductIds ?? [],
				input.States ?? [],
				input.SortBy?.Select(sort => sort.ToSortExpression()).ToArray() ?? [],
				input.SearchTerm),
			cancellationToken);

		return result.Sales.Select(sale => new GqlSale(sale)).ToArray();
	}
}
