using GreenDonut;
using Main.Modules.Balance.Application.Dtos.Balances;
using Main.Modules.Balance.Application.Queries.Balance;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class TransactionDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, TransactionDto>> GetTransactionByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetTransactionsByIdsQuery(keys), cancellationToken)).Transactions;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, decimal>> GetApproximateBalanceByOrganizationIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetApproximateBalancesByOrganizationIdsQuery(keys), cancellationToken))
			.Balances;
}
