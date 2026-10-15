using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Handlers.Characteristics.DeleteCharacteristic;
using Main.Application.Handlers.Characteristics.UpsertCharacteristic;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class CharacteristicMutations
{
	[GraphQLName("upsert")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_DETAILS_EDIT)]
	public async Task<GqlCharacteristic> UpsertAsync(
		ISender sender,
		GqlUpsertCharacteristicInput input,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpsertCharacteristicCommand(input.Id, input.Name),
			cancellationToken);

		return new GqlCharacteristic(result.Characteristic);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_DETAILS_EDIT)]
	public async Task<bool> DeleteAsync(
		ISender sender,
		int id,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteCharacteristicCommand(id), cancellationToken);
		return true;
	}
}
