using Security.Core.Interfaces;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Order;
using Main.Api.GraphQl.Types.Order;
using Main.Application.Orders.Commands.ChangeOrderStatus;
using Main.Application.Orders.Commands.UpsertOrder;
using Main.Enums.Orders;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class OrderMutations
{
	[GraphQLName("upsert")]
	[RequireAllPermissions(PermissionCodes.ORDERS_EDIT)]
	public async Task<GqlOrder> UpsertAsync(
		GqlUpsertOrderInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpsertOrderCommand(input.ToDto()),
			cancellationToken);

		return new GqlOrder(result.OrderId);
	}

	[GraphQLName("confirm")]
	[RequireAllPermissions(PermissionCodes.ORDERS_CONFIRM)]
	public async Task<GqlOrder> ConfirmAsync(
		GqlChangeOrderStatusInput input,
		ISender sender,
		IUserContext userContext,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new ChangeOrderStatusCommand(
				input.OrderId,
				userContext.UserId,
				OrderStatus.Confirmed,
				input.RowVersion),
			cancellationToken);

		return new GqlOrder(input.OrderId);
	}

	[GraphQLName("complete")]
	[RequireAllPermissions(PermissionCodes.ORDERS_CONFIRM)]
	public async Task<GqlOrder> CompleteAsync(
		GqlChangeOrderStatusInput input,
		ISender sender,
		IUserContext userContext,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new ChangeOrderStatusCommand(
				input.OrderId,
				userContext.UserId,
				OrderStatus.Completed,
				input.RowVersion),
			cancellationToken);
		return new GqlOrder(input.OrderId);
	}

	[GraphQLName("cancel")]
	[RequireAllPermissions(PermissionCodes.ORDERS_CANCEL)]
	public async Task<GqlOrder> CancelAsync(
		GqlChangeOrderStatusInput input,
		ISender sender,
		IUserContext userContext,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new ChangeOrderStatusCommand(
				input.OrderId,
				userContext.UserId,
				OrderStatus.Cancelled,
				input.RowVersion),
			cancellationToken);
		return new GqlOrder(input.OrderId);
	}
}
