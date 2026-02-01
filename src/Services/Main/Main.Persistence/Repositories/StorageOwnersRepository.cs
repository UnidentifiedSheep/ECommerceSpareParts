using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class StorageOwnersRepository(DContext context) : IStorageOwnersRepository
{
    public async Task<IEnumerable<Storage>> GetUserStoragesAsync(Guid userId, int? page = null, int? limit = null, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.StorageOwners
            .Include(x => x.StorageNameNavigation)
            .ConfigureTracking(track)
            .Where(u => u.OwnerId == userId)
            .Select(x => x.StorageNameNavigation);

        if (page.HasValue && limit.HasValue)
            query = query.Skip(page.Value * limit.Value).Take(limit.Value);
        
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<StorageOwner?> GetStorageOwnerAsync(Guid userId, string storageName, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageOwners.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.OwnerId == userId && x.StorageName == storageName, cancellationToken);
    }
}