using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserPermissionRepository(DContext context) : IUserPermissionRepository
{
    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, bool track = true, 
        CancellationToken cancellationToken = default)
    {
        var permission = await context.UserPermissions
            .ConfigureTracking(track)
            .Include(x => x.PermissionNavigation)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        return permission.Select(x => x.PermissionNavigation);
    }
    
}