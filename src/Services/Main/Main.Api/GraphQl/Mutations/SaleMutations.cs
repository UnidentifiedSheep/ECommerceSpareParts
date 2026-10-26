using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Sale;
using Main.Application.Handlers.Sales;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Application.Handlers.Sales.EditSale;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class SaleMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.SALES_CREATE)]
	public async Task<GqlSale> CreateAsync(
		GqlCreateSaleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateSaleCommand(
				input.UserId,
				input.OrganizationId,
				input.CurrencyId,
				input.StorageCode,
				input.SaleDateTime,
				input.Contents.Select(content => content.ToDto()).ToArray(),
				input.Comment,
				input.PayedSum,
				input.ConfirmationCode,
				input.ForcePayment ?? false),
			cancellationToken);

		return new GqlSale(result.SaleId);
	}

	[GraphQLName("update")]
	[RequireAllPermissions(PermissionCodes.SALES_EDIT)]
	public async Task<GqlSale> UpdateAsync(
		GqlUpdateSaleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new EditSaleCommand(
				input.SaleId,
				input.RowVersion,
				input.Contents.Select(content => content.ToDto()).ToArray(),
				input.CurrencyId,
				input.SaleDateTime,
				input.Comment,
				input.ConfirmationCode,
				input.ForcePayment ?? false),
			cancellationToken);

		return new GqlSale(input.SaleId);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.SALES_DELETE)]
	public async Task<Guid> DeleteAsync(
		GqlDeleteSaleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteSaleCommand(input.SaleId, input.RowVersion), cancellationToken);
		return input.SaleId;
	}
}
