using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class PurchaseLogisticsRepository(DContext context) : IPurchaseLogisticsRepository
{
    public async Task<IEnumerable<PurchaseLogistic>> GetPurchaseLogistics(IEnumerable<string> ids, bool track = true, 
        CancellationToken token = default)
    {
        return await context.PurchaseLogistics.ConfigureTracking(track)
            .Where(x => ids.Contains(x.PurchaseId))
            .ToListAsync(token);
    }

    public async Task<PurchaseLogistic?> GetPurchaseLogistics(string id, bool track = true, CancellationToken token = default,
        params Expression<Func<PurchaseLogistic, object?>>[] includes)
    {
        var query = context.PurchaseLogistics.ConfigureTracking(track)
            .Where(x => x.PurchaseId == id);

        foreach (var include in includes)
            query = query.Include(include);
        
        return await query.FirstOrDefaultAsync(token);
    }
}