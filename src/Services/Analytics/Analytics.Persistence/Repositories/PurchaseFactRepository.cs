using Abstractions.Models.Repository;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Analytics.Persistence.Repositories;

public class PurchaseFactRepository(DContext context) : IPurchaseFactRepository
{
    public async Task<PurchasesFact?> GetFact(
        QueryOptions<PurchasesFact, string> options,
        CancellationToken cancellationToken = default)
    {
        return await context.PurchasesFacts
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.Id == options.Data, cancellationToken);
    }
}