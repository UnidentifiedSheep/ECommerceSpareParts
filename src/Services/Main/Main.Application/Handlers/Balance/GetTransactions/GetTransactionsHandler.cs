using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Application.Dtos.Balances;
using Main.Entities.Balance;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance.GetTransactions;

public record GetTransactionsQuery(
    RangeModel<DateTime> DateRange,
    int? CurrencyId,
    Guid? SenderId,
    Guid? ReceiverId,
    LogicalOperation LogicalOperation,
    Cursor<(Guid id, DateTime dt)> Cursor,
    bool SkipReversed
) : IQuery<GetTransactionsResult>;

public record GetTransactionsResult(IReadOnlyList<TransactionDto> Transactions);

public class GetTransactionsHandler(
    IReadRepository<Transaction, Guid> repository,
    IProjectionProvider<Transaction, TransactionDto> projection
)
    : IQueryHandler<GetTransactionsQuery, GetTransactionsResult>
{
    private static readonly List<TransactionStatus> ReversedStatuses =
        GetStatusesWithFlag(TransactionStatus.Reversed)
            .ToList();

    public async Task<GetTransactionsResult> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = request.Cursor;
        var query = repository.Query;

        if (request.CurrencyId.HasValue) query = query.Where(e => e.CurrencyId == request.CurrencyId.Value);

        query = request.LogicalOperation switch
        {
            LogicalOperation.Or => ApplyOrUserFilter(
                query,
                request.SenderId,
                request.ReceiverId),
            _ => ApplyAndUserFilter(
                query,
                request.SenderId,
                request.ReceiverId)
        };

        if (request.DateRange.Min.HasValue)
            query = query.Where(x => x.TransactionDatetime >= request.DateRange.Min.Value);

        if (request.DateRange.Max.HasValue)
            query = query.Where(x => x.TransactionDatetime <= request.DateRange.Max.Value);

        if (request.SkipReversed) 
            query = query.Where(x => !ReversedStatuses.Contains(x.Status));

        if (cursor.CursorValue != (Guid.Empty, DateTime.MinValue))
            query = query.Where(x =>
                x.TransactionDatetime < cursor.CursorValue.dt ||
                x.TransactionDatetime == cursor.CursorValue.dt && x.Id < cursor.CursorValue.id);

        var res = await query
            .OrderByDescending(x => x.TransactionDatetime)
            .ThenByDescending(x => x.Id)
            .Take(cursor.Size)
            .Project(projection)
            .ToListAsync(cancellationToken);

        return new GetTransactionsResult(res);
    }

    private static IQueryable<Transaction> ApplyAndUserFilter(
        IQueryable<Transaction> query,
        Guid? senderId,
        Guid? receiverId)
    {
        if (senderId.HasValue) query = query.Where(e => e.SenderId == senderId.Value);

        if (receiverId.HasValue) query = query.Where(e => e.ReceiverId == receiverId.Value);

        return query;
    }

    private static IQueryable<Transaction> ApplyOrUserFilter(
        IQueryable<Transaction> query,
        Guid? senderId,
        Guid? receiverId)
    {
        if (!senderId.HasValue && !receiverId.HasValue) return query;

        if (senderId == receiverId)
        {
            var organizationId = senderId ?? receiverId!.Value;
            return query.Where(e =>
                e.SenderId == organizationId || e.ReceiverId == organizationId);
        }

        return query.Where(e =>
            senderId.HasValue && (e.SenderId == senderId.Value || e.ReceiverId == senderId.Value) ||
            receiverId.HasValue && (e.SenderId == receiverId.Value || e.ReceiverId == receiverId.Value));
    }

    private static TransactionStatus[] GetStatusesWithFlag(TransactionStatus flag)
    {
        var allFlagsMask = Enum.GetValues<TransactionStatus>()
            .Aggregate(0, (current, value) => current | (int)value);

        return Enumerable.Range(0, allFlagsMask + 1)
            .Select(x => (TransactionStatus)x)
            .Where(x => x.HasFlag(flag))
            .ToArray();
    }
}
