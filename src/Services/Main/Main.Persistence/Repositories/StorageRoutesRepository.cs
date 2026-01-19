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
    
    public async Task<StorageRoute?> GetStorageRouteAsync(Guid id, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.StorageRoutes
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}