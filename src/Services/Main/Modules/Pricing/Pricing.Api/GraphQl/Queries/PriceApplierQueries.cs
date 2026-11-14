using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using MediatR;
using Pricing.Api.GraphQl.Types;
using Pricing.Application.Handlers.PriceApplier;
using Pricing.Application.Handlers.PriceApplier.GetPriceAppliers;
using Pricing.Enums;

namespace Pricing.Api.GraphQl.Queries;

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
