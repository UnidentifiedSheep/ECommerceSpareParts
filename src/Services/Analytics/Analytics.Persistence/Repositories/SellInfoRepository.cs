using System.Linq.Expressions;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Analytics.Persistence.Repositories;

public class SellInfoRepository(DContext context) : ISellInfoRepository
{
    public IAsyncEnumerable<SellInfo> GetSellInfos(Expression<Func<SellInfo, bool>> where, bool track = true)
    {
        return context.SellInfos.ConfigureTracking(track)
            .Where(where)
            .OrderBy(x => x.SellContentId)
            .AsAsyncEnumerable();
    }

    public async Task<IEnumerable<SellInfo>> GetSellInfosList(Expression<Func<SellInfo, bool>> where, bool track = true, 
        CancellationToken cancellationToken = default)
    {
        return await context.SellInfos.ConfigureTracking(track)
            .Where(where)
            .OrderBy(x => x.SellContentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<SellInfo?> GetWithMaximumBuyPrice(bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.SellInfos.ConfigureTracking(track)
            .OrderByDescending(x => x.BuyPrices)
            .FirstOrDefaultAsync(cancellationToken);
    }
}