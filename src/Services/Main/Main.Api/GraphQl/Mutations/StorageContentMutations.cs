using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Storage;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.EditContent;
using Main.Application.Handlers.StorageContents.SetToZeroContent;
using Main.Enums;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class StorageContentMutations
{
	[GraphQLName("add")]
	[RequireAllPermissions(PermissionCodes.STORAGES_CONTENT_CREATE)]
	public async Task<bool> AddAsync(
		GqlAddStorageContentInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new AddContentCommand(
				input.Items.Select(item => item.ToDto()).ToArray(),
				input.StorageCode,
				StorageMovementType.StorageContentAddition),
			cancellationToken);
		return true;
	}

	[GraphQLName("edit")]
	[RequireAllPermissions(PermissionCodes.STORAGES_CONTENT_EDIT)]
	public async Task<bool> EditAsync(
		IReadOnlyCollection<GqlEditStorageContentInput> items,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new EditStorageContentCommand(
				items.ToDictionary(
					item => item.Id,
					item => new ModelWithRowVersion<PatchStorageContentDto, uint>(item.ToDto(), item.RowVersion))),
			cancellationToken);
		return true;
	}

	[GraphQLName("setToZero")]
	[RequireAllPermissions(PermissionCodes.STORAGES_CONTENT_DELETE)]
	public async Task<bool> SetToZeroAsync(
		int contentId,
		uint rowVersion,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new SetToZeroContentCommand(contentId, rowVersion), cancellationToken);
		return true;
	}
}
