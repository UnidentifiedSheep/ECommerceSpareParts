using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using MediatR;
using Pricing.Api.GraphQl.Types;
using Pricing.Api.GraphQl.Types.Inputs;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Handlers.PriceApplier;
using Pricing.Application.Handlers.PriceApplier.DeletePriceApplier;
using Pricing.Application.Handlers.PriceApplier.UpsertPriceApplier;

namespace Pricing.Api.GraphQl.Mutations;

public sealed class PriceApplierMutations
{
	[GraphQLName("upsert")]
	[RequireAllPermissions(PermissionCodes.PRICE_APPLIERS_MANAGE)]
	public async Task<GqlPriceApplier> UpsertAsync(
		GqlUpsertPriceApplierInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new UpsertPriceApplierCommand(
			input.SystemName,
			input.Name,
			input.DslLogic,
			input.States.Select(state => new UpsertPriceApplierStateDto
			{
				Usage = state.Usage,
				Order = state.Order,
				Enabled = state.Enabled
			}).ToArray()), cancellationToken);
		var applier = await sender.Send(new GetPriceApplierQuery(result.SystemName), cancellationToken);
		return new GqlPriceApplier(applier.Applier);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.PRICE_APPLIERS_MANAGE)]
	public async Task<bool> DeleteAsync(
		string systemName,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeletePriceApplierCommand(systemName), cancellationToken);
		return true;
	}
}
