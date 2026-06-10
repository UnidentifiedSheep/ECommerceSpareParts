using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Application.Handlers.Projections;
using Main.Entities.Balance;
using Enums;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance.GetTransactions;

public record GetTransactionsQuery(
    DateTime RangeStart,
    DateTime RangeEnd,
    int? CurrencyId,
    Guid? SenderId,
    Guid? ReceiverId,
    LogicalOperation LogicalOperation,
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

        query = request.LogicalOperation switch
        {
            LogicalOperation.Or => ApplyOrUserFilter(query, request.SenderId, request.ReceiverId),
            _ => ApplyAndUserFilter(query, request.SenderId, request.ReceiverId)
        };

        var fixedStart = request.RangeStart.Date;
        var fixedEnd = request.RangeEnd.Date.AddDays(1);
        query = query.Where(x => x.TransactionDatetime >= fixedStart &&
                                 x.TransactionDatetime <= fixedEnd);

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

    private static IQueryable<Transaction> ApplyAndUserFilter(
        IQueryable<Transaction> query,
        Guid? senderId,
        Guid? receiverId)
    {
        if (senderId.HasValue)
            query = query.Where(e => e.SenderId == senderId.Value);

        if (receiverId.HasValue)
            query = query.Where(e => e.ReceiverId == receiverId.Value);

        return query;
    }

    private static IQueryable<Transaction> ApplyOrUserFilter(
        IQueryable<Transaction> query,
        Guid? senderId,
        Guid? receiverId)
    {
        if (!senderId.HasValue && !receiverId.HasValue)
            return query;

        if (senderId == receiverId)
        {
            var userId = senderId ?? receiverId!.Value;
            return query.Where(e => e.SenderId == userId || e.ReceiverId == userId);
        }

        return query.Where(e =>
            senderId.HasValue && (e.SenderId == senderId.Value || e.ReceiverId == senderId.Value) ||
            receiverId.HasValue && (e.SenderId == receiverId.Value || e.ReceiverId == receiverId.Value));
    }
}
