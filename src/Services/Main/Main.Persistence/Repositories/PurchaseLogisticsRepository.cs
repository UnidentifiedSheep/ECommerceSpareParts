using System.Linq.Expressions;
using Abstractions.Models.Repository;
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

    public async Task<PurchaseLogistic?> GetPurchaseLogistics(string id, QueryOptions? config = null, 
        CancellationToken token = default)
    {
        return await context.PurchaseLogistics
            .ApplyOptions(config)
            .Where(x => x.PurchaseId == id)
            .FirstOrDefaultAsync(token);
    }
}