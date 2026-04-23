using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Storage;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Storage;

public class StorageRouteRepository(DContext context) : RepositoryBase<DContext, StorageRoute, Guid>(context), IStorageRouteRepository
{
    public async Task<StorageRoute?> GetActiveRouteAsync(
        string from, 
        string to, 
        Criteria<StorageRoute>? criteria = null, 
        CancellationToken ct = default)
    {
        var query = Context.StorageRoutes.AsQueryable();

        if (criteria != null)
            query = query.Apply(criteria);

        return await query
            .FirstOrDefaultAsync(
                x => x.FromStorageName == from 
                     && x.ToStorageName == to 
                     && x.IsActive, ct);
    }

    public async Task<bool> IsAnyRouteActiveAsync(string from, string to, CancellationToken ct = default)
    {
        return await Context.StorageRoutes
            .AnyAsync(x => 
                x.FromStorageName == from && 
                x.ToStorageName == to && 
                x.IsActive, ct); 
    }
}