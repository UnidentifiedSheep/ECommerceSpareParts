using GreenDonut;
using Main.Application.Dtos.Balances;
using Main.Application.Handlers.Balance;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class TransactionDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, TransactionDto>> GetTransactionByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken) =>
		(await sender.Send(new GetTransactionsByIdsQuery(keys), cancellationToken)).Transactions;
}
