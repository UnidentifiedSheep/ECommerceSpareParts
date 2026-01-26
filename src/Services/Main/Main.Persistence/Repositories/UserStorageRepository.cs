using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserStorageRepository(DContext context) : IUserStorageRepository
{
    public async Task<IEnumerable<Storage>> GetUserStoragesAsync(Guid userId, int? page = null, int? limit = null, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.Users
            .ConfigureTracking(track)
            .Where(u => u.Id == userId)
            .SelectMany(u => u.StorageNames);

        if (page.HasValue && limit.HasValue)
            query = query.Skip(page.Value * limit.Value).Take(limit.Value);
        
        return await query.ToListAsync(cancellationToken);
    }
}