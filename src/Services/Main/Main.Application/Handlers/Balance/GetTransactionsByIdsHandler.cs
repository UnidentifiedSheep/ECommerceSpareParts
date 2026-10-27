using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Balances;
using Main.Entities.Balance;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance;

public sealed record GetTransactionsByIdsQuery : IQuery<GetTransactionsByIdsResult>
{
	public GetTransactionsByIdsQuery(Guid id) : this([id]) { }

	public GetTransactionsByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetTransactionsByIdsResult(IReadOnlyDictionary<Guid, TransactionDto> Transactions);

public sealed class GetTransactionsByIdsHandler(
	IReadRepository<Transaction, Guid> repository,
	IProjectionProvider<Transaction, TransactionDto> projection)
	: IQueryHandler<GetTransactionsByIdsQuery, GetTransactionsByIdsResult>
{
	public async Task<GetTransactionsByIdsResult> Handle(
		GetTransactionsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetTransactionsByIdsResult(new Dictionary<Guid, TransactionDto>());

		var transactions = await repository.Query
			.Where(transaction => request.Ids.Contains(transaction.Id))
			.Project(projection)
			.ToDictionaryAsync(transaction => transaction.Id, cancellationToken);

		return new GetTransactionsByIdsResult(transactions);
	}
}
