using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Pricing.Types;
using MediatR;
using Pricing.Application.Queries.PriceApplier;
using Pricing.Application.Queries.PriceApplier.GetPriceAppliers;
using Pricing.Entities.Enums;

namespace Main.Api.GraphQl.Pricing.Queries;

public sealed class PriceApplierQueries
{
	[GraphQLName("bySystemName")]
	[RequireAllPermissions(PermissionCodes.PRICE_APPLIERS_MANAGE)]
	public async Task<GqlPriceApplier> GetBySystemNameAsync(
		string systemName,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetPriceApplierQuery(systemName), cancellationToken);
		return new GqlPriceApplier(result.Applier);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.PRICE_APPLIERS_MANAGE)]
	public async Task<IReadOnlyList<GqlPriceApplier>> GetListAsync(
		PriceOfferSourceType usage,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetPriceAppliersQuery(usage), cancellationToken);
		return result.Appliers.Select(applier => new GqlPriceApplier(applier)).ToArray();
	}
}
