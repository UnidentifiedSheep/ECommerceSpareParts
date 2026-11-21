using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Transaction;
using Main.Entities.Balance.Enums.Balances;
using Main.Modules.Balance.Application.Commands.Balance;
using Main.Modules.Balance.Application.Commands.Balance.ReverseTransaction;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class TransactionMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_CREATE)]
	public async Task<GqlTransaction> CreateAsync(
		GqlCreateTransactionInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateSystemTransactionCommand(
				input.OrganizationId,
				input.Amount,
				input.CurrencyId,
				input.TransactionDateTime,
				input.Direction,
				input.ForcePayment ?? false),
			cancellationToken);

		return new GqlTransaction(result.Transaction.Id);
	}

	[GraphQLName("reverse")]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_DELETE)]
	public async Task<GqlTransaction> ReverseAsync(
		GqlReverseTransactionInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new ReverseTransactionCommand(
				input.TransactionId,
				TransactionReversalMode.User,
				input.ForcePayment ?? false),
			cancellationToken);

		return new GqlTransaction(result.Transaction.Id);
	}
}
