using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Pricing.Types;
using Main.Api.GraphQl.Pricing.Types.Inputs;
using MediatR;
using Pricing.Application.Commands.Markup;
using Pricing.Application.Commands.Markup.UpsertMarkupGroup;
using Pricing.Application.Dtos.Markup;

namespace Main.Api.GraphQl.Pricing.Mutations;

public sealed class MarkupGroupMutations
{
	[GraphQLName("upsert")]
	[RequireAllPermissions(PermissionCodes.MARKUP_CREATE)]
	public async Task<GqlMarkupGroup> UpsertAsync(
		GqlUpsertMarkupGroupInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpsertMarkupGroupCommand(
				new UpsertMarkupGroupDto
				{
					Id = input.Id,
					Name = input.Name,
					CurrencyId = input.CurrencyId,
					Ranges = input.Ranges
						.Select(range => new UpsertMarkupRangeDto
						{
							RangeStart = range.RangeStart,
							RangeEnd = range.RangeEnd,
							Markup = range.Markup
						})
						.ToArray()
				}),
			cancellationToken);
		return new GqlMarkupGroup(result.Id);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.MARKUP_DELETE)]
	public async Task<bool> DeleteAsync(
		int id,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteMarkupCommand(id), cancellationToken);
		return true;
	}
}
