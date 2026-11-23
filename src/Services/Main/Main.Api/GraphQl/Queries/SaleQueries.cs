using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Inputs.Sale;
using Main.Api.GraphQl.Types.Sale;
using Main.Modules.Sale.Application.Queries.Sales.GetSale;
using Main.Modules.Sale.Application.Models;
using Main.Modules.Sale.Application.Queries.Sales.GetSales;
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
				new SalesFilter
				{
					DateRange = new RangeModel<DateTime>(input.RangeStartDate, input.RangeEndDate),
					OrganizationIds = input.OrganizationIds ?? [],
					CurrencyIds = input.CurrencyIds ?? [],
					ProductIds = input.ProductIds ?? [],
					States = input.States ?? [],
					SortBy = input.SortBy.ToSortExpressions(),
					SearchTerm = input.SearchTerm
				},
				input.Pagination),
			cancellationToken);

		return result.Sales.Select(sale => new GqlSale(sale)).ToArray();
	}
}
