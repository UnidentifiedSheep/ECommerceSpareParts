using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Pricing.Types;
using Main.Api.GraphQl.Pricing.Types.Inputs;
using MediatR;
using Pricing.Application.Commands.PriceApplier.DeletePriceApplier;
using Pricing.Application.Commands.PriceApplier.UpsertPriceApplier;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Queries.PriceApplier;

namespace Main.Api.GraphQl.Pricing.Mutations;

public sealed class PriceApplierMutations
{
	[GraphQLName("upsert")]
	[RequireAllPermissions(PermissionCodes.PRICE_APPLIERS_MANAGE)]
	public async Task<GqlPriceApplier> UpsertAsync(
		GqlUpsertPriceApplierInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpsertPriceApplierCommand(
				input.SystemName,
				input.Name,
				input.DslLogic,
				input.States
					.Select(state => new UpsertPriceApplierStateDto
					{
						Usage = state.Usage,
						Order = state.Order,
						Enabled = state.Enabled
					})
					.ToArray()),
			cancellationToken);
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
