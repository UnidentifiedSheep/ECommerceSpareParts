using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Purchase;
using Main.Application.Handlers.Purchases.DeletePurchase;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.Purchases.EditPurchase;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class PurchaseMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_CREATE)]
	public async Task<GqlPurchase> CreateAsync(
		GqlCreatePurchaseInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreatePurchaseCommand(
				input.SupplierUserId,
				input.SupplierOrganizationId,
				input.CurrencyId,
				input.StorageCode,
				input.PurchaseDate,
				input.Contents.Select(content => content.ToDto()).ToArray(),
				input.Comment,
				input.PayedSum,
				input.WithLogistics ?? false,
				input.StorageFrom,
				input.ForcePayment ?? false),
			cancellationToken);

		return new GqlPurchase(result.PurchaseId);
	}

	[GraphQLName("update")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_EDIT)]
	public async Task<GqlPurchase> UpdateAsync(
		GqlUpdatePurchaseInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new EditPurchaseCommand(
				input.Contents.Select(content => content.ToDto()).ToArray(),
				input.PurchaseId,
				input.CurrencyId,
				input.Comment,
				input.PurchaseDateTime,
				input.WithLogistics ?? false,
				input.StorageFrom),
			cancellationToken);

		return new GqlPurchase(input.PurchaseId);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.PURCHASE_DELETE)]
	public async Task<Guid> DeleteAsync(
		GqlDeletePurchaseInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeletePurchaseCommand(input.PurchaseId), cancellationToken);
		return input.PurchaseId;
	}
}
