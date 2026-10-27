using Abstractions.Models;
using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Transaction;
using Main.Application.Handlers.Balance.GetTransactions;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class TransactionQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_GET_ALL)]
	public async Task<GqlTransaction?> GetByIdAsync(
		Guid id,
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var transaction = await loader.LoadAsync(id, cancellationToken);
		return transaction is null ? null : new GqlTransaction(transaction);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_GET_ALL)]
	public async Task<IReadOnlyList<GqlTransaction>> ListAsync(
		GqlGetTransactionsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var cursor = input.Cursor.Value;
		var result = await sender.Send(
			new GetTransactionsQuery(
				new RangeModel<DateTime>(input.RangeStart, input.RangeEnd),
				input.CurrencyId,
				input.SenderId,
				input.ReceiverId,
				input.LogicalOperation ?? LogicalOperation.And,
				new Cursor<(Guid id, DateTime dt)>(
					cursor is null ? (Guid.Empty, DateTime.MinValue) : (cursor.Value.Id, cursor.Value.TransactionDate),
					input.Cursor.Size),
				input.SkipReversed ?? false),
			cancellationToken);

		return result.Transactions.Select(transaction => new GqlTransaction(transaction)).ToArray();
	}
}
