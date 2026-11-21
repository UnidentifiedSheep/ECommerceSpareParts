using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Storage;
using Main.Modules.Storage.Application.Commands.Storages.CreateStorage;
using Main.Modules.Storage.Application.Commands.Storages.DeleteStorage;
using Main.Modules.Storage.Application.Commands.Storages.EditStorage;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class StorageMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.STORAGES_CREATE)]
	public async Task<GqlStorage> CreateAsync(
		GqlCreateStorageInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateStorageCommand(
				input.Code,
				input.Description,
				input.Location,
				input.Type),
			cancellationToken);

		return new GqlStorage(result.Code);
	}

	[GraphQLName("edit")]
	[RequireAllPermissions(PermissionCodes.STORAGES_EDIT)]
	public async Task<GqlStorage> EditAsync(
		string code,
		GqlPatchStorageInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new EditStorageCommand(code, input.ToDto()), cancellationToken);
		return new GqlStorage(code);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.STORAGES_DELETE)]
	public async Task<bool> DeleteAsync(
		string code,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteStorageCommand(code), cancellationToken);
		return true;
	}
}
