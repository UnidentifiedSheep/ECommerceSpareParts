using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class PurchaseLogisticsRepository(DContext context) : IPurchaseLogisticsRepository
{
    public async Task<IEnumerable<PurchaseLogistic>> GetPurchaseLogistics(
        QueryOptions<PurchaseLogistic, IReadOnlyList<string>> options,
        CancellationToken token = default)
    {
        return await context.PurchaseLogistics
            .ApplyOptions(options)
            .Where(x => options.Data.Contains(x.PurchaseId))
            .ToListAsync(token);
    }

    public async Task<PurchaseLogistic?> GetPurchaseLogistics(
        QueryOptions<PurchaseLogistic, string> options,
        CancellationToken token = default)
    {
        return await context.PurchaseLogistics
            .ApplyOptions(options)
            .Where(x => x.PurchaseId == options.Data)
            .FirstOrDefaultAsync(token);
    }
}