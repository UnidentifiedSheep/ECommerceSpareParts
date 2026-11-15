using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Producer;
using Main.Api.GraphQl.Types.Producer;
using Main.Application.Handlers.ProducerAliases;
using Main.Application.Handlers.ProducerAliases.AddAlias;
using Main.Application.Handlers.Producers;
using Main.Application.Handlers.Producers.CreateProducer;
using Main.Application.Handlers.Producers.EditProducer;
using Main.Application.Handlers.ProducerSupplierMappings;
using Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class ProducerMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.PRODUCERS_CREATE)]
	public async Task<GqlProducer> CreateAsync(
		GqlCreateProducerInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new CreateProducerCommand(input.ToDto()), cancellationToken);
		return new GqlProducer(result.ProducerId);
	}

	[GraphQLName("edit")]
	[RequireAllPermissions(PermissionCodes.PRODUCERS_EDIT)]
	public async Task<GqlProducer> EditAsync(
		int producerId,
		GqlPatchProducerInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new EditProducerCommand(producerId, input.ToDto()), cancellationToken);
		return new GqlProducer(result.ProducerId);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.PRODUCERS_DELETE)]
	public async Task<bool> DeleteAsync(int id, ISender sender, CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteProducerCommand(id), cancellationToken);
		return true;
	}

	[GraphQLName("addAlias")]
	[RequireAllPermissions(PermissionCodes.PRODUCERS_EDIT)]
	public async Task<GqlProducer> AddAliasAsync(
		GqlProducerAliasInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new AddAliasCommand(input.ProducerId, input.Alias), cancellationToken);
		return new GqlProducer(input.ProducerId);
	}

	[GraphQLName("deleteAlias")]
	[RequireAllPermissions(PermissionCodes.PRODUCERS_EDIT)]
	public async Task<GqlProducer> DeleteAliasAsync(
		GqlProducerAliasInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteAliasCommand(input.ProducerId, input.Alias), cancellationToken);
		return new GqlProducer(input.ProducerId);
	}

	[GraphQLName("createSupplierMapping")]
	[RequireAllPermissions(PermissionCodes.PRODUCERS_EDIT)]
	public async Task<GqlProducerSupplierMapping> CreateSupplierMappingAsync(
		GqlCreateProducerSupplierMappingInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateProducerSupplierMappingCommand(input.ToDto()),
			cancellationToken);
		return new GqlProducerSupplierMapping(result.ProducerSupplierMapping);
	}

	[GraphQLName("deleteSupplierMapping")]
	[RequireAllPermissions(PermissionCodes.PRODUCERS_EDIT)]
	public async Task<bool> DeleteSupplierMappingAsync(
		int id,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteProducerSupplierMappingCommand(id), cancellationToken);
		return true;
	}
}
