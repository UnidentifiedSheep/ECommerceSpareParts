using Core.Entities;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class BalanceRepository(DContext context) : IBalanceRepository
{
    public async Task<Transaction?> GetTransactionByIdAsync(string id, bool track = true, CancellationToken ct = default)
    {
        return await context.Transactions
            .FromSql($"SELECT * FROM transactions WHERE id = {id} FOR UPDATE")
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> TransactionExistsAsync(string senderId, string receiverId, DateTime dt, string? exceptId = null, CancellationToken ct = default)
    {
        return await context.Transactions.AsNoTracking()
            .AnyAsync(x => x.SenderId == senderId && x.ReceiverId == receiverId &&
                           x.TransactionDatetime == dt && (exceptId == null || x.Id != exceptId), ct);
    }

    public async Task<Transaction?> GetPreviousTransactionAsync(DateTime dt, string userId, int currencyId, bool track = true, CancellationToken ct = default)
    {
        var sql = """
                      SELECT * FROM transactions
                      WHERE transaction_datetime < @dt
                        AND (sender_id = @userId OR receiver_id = @userId)
                        AND currency_id = @currencyId
                      ORDER BY transaction_datetime DESC
                      LIMIT 1
                      FOR UPDATE
                  """;

        var dtParam = new NpgsqlParameter("dt", dt);
        var userIdParam = new NpgsqlParameter("userId", userId);
        var currencyIdParam = new NpgsqlParameter("currencyId", currencyId);

        return await context.Transactions
            .FromSqlRaw(sql, dtParam, userIdParam, currencyIdParam)
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(ct);
    }

    public IAsyncEnumerable<Transaction> GetAffectedTransactions(string userId, int currencyId, DateTime dt, string? excludeId = null, bool track = true)
    {
        var sql = excludeId is null
            ? """
                  SELECT * FROM transactions 
                  WHERE currency_id = @currencyId
                    AND transaction_datetime >= @dt
                    AND (sender_id = @userId OR receiver_id = @userId)
                    AND is_deleted = false
                  ORDER BY transaction_datetime DESC, id DESC
                  FOR UPDATE
              """
            : """
                  SELECT * FROM transactions 
                  WHERE currency_id = @currencyId
                    AND transaction_datetime >= @dt
                    AND (sender_id = @userId OR receiver_id = @userId)
                    AND is_deleted = false
                    AND id != @excludeId
                  ORDER BY transaction_datetime DESC, id DESC
                  FOR UPDATE
              """;

        var parameters = excludeId is null
            ? new object[]
            {
                new NpgsqlParameter("@currencyId", currencyId),
                new NpgsqlParameter("@dt", dt),
                new NpgsqlParameter("@userId", userId),
            }
            : new object[]
            {
                new NpgsqlParameter("@currencyId", currencyId),
                new NpgsqlParameter("@dt", dt),
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@excludeId", excludeId)
            };

        return context.Transactions.FromSqlRaw(sql, parameters)
            .ConfigureTracking(track)
            .AsAsyncEnumerable();
    }

    public async Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default)
    {
        await context.Transactions.AddAsync(transaction, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task<UserBalance?> GetUserBalanceAsync(string userId, int currencyId, bool track = true, CancellationToken ct = default)
    {
        var sql = """
                      SELECT * FROM user_balances
                      WHERE user_id = @userId AND currency_id = @currencyId
                      FOR UPDATE
                  """;

        var userIdParam = new NpgsqlParameter("@userId", userId);
        var currencyIdParam = new NpgsqlParameter("@currencyId", currencyId);

        return await context.UserBalances
            .FromSqlRaw(sql, userIdParam, currencyIdParam)
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<TransactionVersion?> GetLastTransactionVersionAsync(string transactionId, bool track = true, CancellationToken ct = default)
    {
        return await context.TransactionVersions
            .AsNoTracking()
            .Where(x => x.TransactionId == transactionId)
            .OrderByDescending(x => x.Version)
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime rangeStart, DateTime rangeEnd,
        int? currencyId, string? senderId, string? receiverId, int page, int viewCount, bool track = true, CancellationToken ct = default)
    {
        var query = context.Transactions.ConfigureTracking(track)
            .Where(x => (rangeStart <= x.TransactionDatetime && rangeEnd >= x.TransactionDatetime) &&
                        (currencyId == null || x.CurrencyId == currencyId));
        if (!string.IsNullOrWhiteSpace(senderId) && !string.IsNullOrWhiteSpace(receiverId))
            query = query.Where(x => (x.SenderId == senderId && x.ReceiverId == receiverId) || 
                                     (x.SenderId == receiverId && x.ReceiverId == senderId));
        else
            query = query.Where(x => x.SenderId == senderId || x.ReceiverId == senderId && string.IsNullOrWhiteSpace(senderId))
                .Where(x => x.SenderId == receiverId || x.ReceiverId == receiverId && string.IsNullOrWhiteSpace(receiverId));
        
        
        query = query.OrderBy(x => new {x.TransactionDatetime, x.Id})
            .Skip(page * viewCount)
            .Take(viewCount);
        var result = await query.ToListAsync(ct);
        return result;
    }

    public async Task<bool> TransactionExistsAsync(string transactionId, CancellationToken ct = default)
    {
        return await context.Transactions.AsNoTracking().AnyAsync(x => x.Id == transactionId, ct);
    }
}