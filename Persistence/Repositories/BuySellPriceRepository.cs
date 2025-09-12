using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;
using Persistence.TransactionBuilder;

namespace Persistence.Repositories;

public class BuySellPriceRepository(DContext context) : IBuySellPriceRepository
{
    public async Task<IEnumerable<BuySellPrice>> GetBsPriceByContentIdsForUpdate(IEnumerable<int> contentIds, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.BuySellPrices
            .FromSql($"""
                      select * from buy_sell_prices 
                      where sale_content_id = ANY({contentIds})
                      for update
                      """)
            .ToListAsync(cancellationToken);
    }

    public async IAsyncEnumerable<BuySellPrice> GetBuySellPrices(Expression<Func<BuySellPrice, bool>>? where = null, bool track = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = context.BuySellPrices.Where(where ?? (_ => true)).ConfigureTracking(track);
        await foreach (var item in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
            yield return item;
    }

    public async Task<List<BuySellPrice>> GetBuySellPricesAsList(Expression<Func<BuySellPrice, bool>>? where = null, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.BuySellPrices
            .ConfigureTracking(track)
            .Where(where ?? (_ => true))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRange(IEnumerable<BuySellPrice> buySellPrices, CancellationToken cancellationToken = default)
    {
        await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
                await context.BulkUpdateAsync(buySellPrices, cancellationToken: cancellationToken), cancellationToken);
    }

    public async Task<decimal?> GetMaxBuyPrice(CancellationToken cancellationToken = default)
    {
        if (!await context.BuySellPrices.AnyAsync(cancellationToken))
            return null;
        return await context.BuySellPrices.MaxAsync(x => x.BuyPrice,cancellationToken);
    }
}