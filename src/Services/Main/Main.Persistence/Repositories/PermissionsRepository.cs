using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class PermissionsRepository(DContext context) : IPermissionRepository
{
    public async Task<IEnumerable<Permission>> GetPermissionsAsync(int page, int limit, bool track = true, 
        CancellationToken cancellationToken = default)
    {
        return await context.Permissions
            .ConfigureTracking(track)
            .OrderBy(x => x.Name)
            .Skip(page * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Permission?> GetPermissionAsync(string name, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.Permissions.ConfigureTracking(track).FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }
}