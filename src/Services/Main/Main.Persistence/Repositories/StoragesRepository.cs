using System.Linq.Expressions;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class StoragesRepository(DContext context) : IStoragesRepository
{
    public async Task<Storage?> GetStorageAsync(string name, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<Storage, object>>[] includes)
    {
        var query = context.Storages.ConfigureTracking(track);
        foreach (var include in includes) query = query.Include(include);
        return await query.FirstOrDefaultAsync(x => x.Name == name.Trim(), cancellationToken);
    }

    public async Task<IEnumerable<Storage>> GetStoragesAsync(string? searchTerm, int page, int viewCount,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.Storages.ConfigureTracking(track);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Select(x => new
                {
                    Entity = x,
                    Rank =
                        (EF.Functions.ILike(x.Name, $"%{searchTerm}%") ? 3 : 0) +
                        (x.Description != null && EF.Functions.ILike(x.Description, $"%{searchTerm}%") ? 2 : 0) +
                        (x.Location != null && EF.Functions.ILike(x.Location, $"%{searchTerm}%") ? 1 : 0)
                })
                .Where(x => x.Rank > 0)
                .OrderByDescending(x => x.Rank)
                .Select(x => x.Entity);

        return await query
            .Skip(page * viewCount)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> StorageExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Storages.AsNoTracking().AnyAsync(x => x.Name == name.Trim(), cancellationToken);
    }
    
}