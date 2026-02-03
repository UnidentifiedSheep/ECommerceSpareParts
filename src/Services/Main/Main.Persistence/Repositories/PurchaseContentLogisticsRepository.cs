using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class PurchaseContentLogisticsRepository(DContext context) : IPurchaseContentLogisticsRepository
{
    public async Task<IEnumerable<PurchaseContentLogistic>> GetPurchaseContentLogistics(IEnumerable<int> ids, 
        bool track = true, CancellationToken token = default, params Expression<Func<PurchaseContentLogistic, object>>[] includes)
    {
        var query = context.PurchaseContentLogistics
            .ConfigureTracking(track)
            .Where(x => ids.Contains(x.PurchaseContentId));
        
        foreach (var include in includes)
            query = query.Include(include);
        
        return await query.ToListAsync(token);
    }
}