using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class StorageRoutesRepository(DContext context) : IStorageRoutesRepository
{
    public async Task<StorageRoute?> GetStorageRouteAsync(string storageFrom, string storageTo, bool isActive, 
        bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.StorageRoutes
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.FromStorageName == storageFrom 
                                      && x.ToStorageName == storageTo 
                                      && x.IsActive == isActive, cancellationToken);
    }
    
    public async Task<StorageRoute?> GetStorageRouteAsync(Guid id, bool track = true, 
        CancellationToken cancellationToken = default, params Expression<Func<StorageRoute,object>>[] includes)
    {
        var query = context.StorageRoutes.ConfigureTracking(track);
        foreach (var include in includes)
            query = query.Include(include);
                
        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StorageRoute>> GetStorageRoutesAsync(string? storageFrom, string? storageTo,
        bool? isActive, int? page = null, int? limit = null, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<StorageRoute,object>>[] includes)
    {
        var query = context.StorageRoutes.ConfigureTracking(track);
        if (!string.IsNullOrWhiteSpace(storageFrom))
            query = query.Where(x => x.FromStorageName == storageFrom);
        if (!string.IsNullOrWhiteSpace(storageTo))
            query = query.Where(x => x.ToStorageName == storageTo);
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);
        if (page is >= 0 && limit is > 0)
            query = query.Skip(page.Value * limit.Value).Take(limit.Value);
        
        foreach (var include in includes)
            query = query.Include(include);
        return await query
            .OrderByDescending(x  => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsAnyActive(string storageFrom, string storageTo,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageRoutes
            .AsNoTracking()
            .AnyAsync(x => x.FromStorageName == storageFrom &&
                           x.ToStorageName == storageTo &&
                           x.IsActive, cancellationToken);
    }
}