using Abstractions.Models;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Amw.Balances;
using Main.Application.Handlers.Projections;
using Main.Entities.Balance;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance.GetTransactions;

public record GetTransactionsQuery(
    DateTime RangeStart,
    DateTime RangeEnd,
    int? CurrencyId,
    Guid? SenderId,
    Guid? ReceiverId,
    Cursor<(Guid id, DateTime dt)> Cursor) : IQuery<GetTransactionsResult>;

public record GetTransactionsResult(IReadOnlyList<TransactionDto> Transactions);

public class GetTransactionsHandler(
    IReadRepository<Transaction, Guid> repository)
    : IQueryHandler<GetTransactionsQuery, GetTransactionsResult>
{
    public async Task<GetTransactionsResult> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var cursor = request.Cursor;
        var query = repository.Query;

        if (request.CurrencyId.HasValue)
            query = query.Where(e => e.CurrencyId == request.CurrencyId.Value);
        
        if (request.SenderId.HasValue)
            query = query.Where(e => e.SenderId == request.SenderId.Value);
        
        if (request.ReceiverId.HasValue)
            query = query.Where(e => e.ReceiverId == request.ReceiverId.Value);

        var res = await query
            .Where(x =>
                x.TransactionDatetime > cursor.CursorValue.dt ||
                (x.TransactionDatetime == cursor.CursorValue.dt && x.Id > cursor.CursorValue.id))
            .OrderByDescending(x => x.TransactionDatetime)
            .ThenByDescending(x => x.Id)
            .Take(cursor.Size)
            .AsExpandable()
            .Select(BalanceProjections.ToTransactionDto)
            .ToListAsync(cancellationToken);
        
        return new GetTransactionsResult(res);
    }
}