using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Transaction;
using Main.Entities.User;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class BalanceRepository(DContext context) : IBalanceRepository
{
    public IAsyncEnumerable<Transaction> GetAffectedTransactions(
        Guid userId,
        int currencyId,
        DateTime dt,
        Guid? excludeId = null,
        bool track = true)
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
                new NpgsqlParameter("@dt", dt.ToUniversalTime())
                {
                    NpgsqlDbType = NpgsqlDbType.TimestampTz
                },
                new NpgsqlParameter("@userId", userId)
            }
            : new object[]
            {
                new NpgsqlParameter("@currencyId", currencyId),
                new NpgsqlParameter("@dt", dt.ToUniversalTime())
                {
                    NpgsqlDbType = NpgsqlDbType.TimestampTz
                },
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@excludeId", excludeId)
                {
                    NpgsqlDbType = NpgsqlDbType.Uuid
                }
            };

        return context.Transactions.FromSqlRaw(sql, parameters)
            .ConfigureTracking(track)
            .AsAsyncEnumerable();
    }

    public async Task<UserBalance?> GetUserBalanceAsync(
        Guid userId,
        int currencyId,
        bool track = true,
        CancellationToken ct = default)
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
}